using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Requests;
using Openlysis.Application.Phones.Services;
using Openlysis.Assessors.Shared.Configuration;
using Openlysis.Assessors.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.Logging.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.RateQuota.Models;

namespace Openlysis.Assessors.Shared.Abstractions;

/// <summary>
/// Assess the reputation of a <see cref="TData"/> model.
/// </summary>
/// <typeparam name="TData">The type of data to be assessed.</typeparam>
/// <typeparam name="TModel">The type of model to be returned after assessment.</typeparam>
public abstract class ReputationAssessor<TData, TModel>
    : IReputationAssessor<TData, TModel>, IDisposable
    where TData : AssessData
    where TModel : notnull
{
    /// <inheritdoc/>
    public string ServiceName => _options.CurrentValue.ServiceName;

    /// <inheritdoc/>
    public bool IsAvailable { get; private set; } = true;

    private readonly ServiceLogger _logger;
    private readonly IOptionsMonitor<DataAssessorOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRateQuotaService<AssessorEndpointType>? _rateQuotaService;
    private readonly IEndpointAddressFactory<TData> _endpointAddressFactory;
    private readonly IResponseParser<TModel> _responseParser;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReputationAssessor{TData,TModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options monitor for accessing configuration settings.</param>
    /// <param name="httpClientFactory">The factory to create HTTP clients.</param>
    /// <param name="rateQuotaService">The service to manage rate quotas for assessor endpoints.</param>
    /// <param name="endpointAddressFactory">The factory to create endpoint addresses for the given data type.</param>
    /// <param name="responseParser">The parser to parse the HTTP response into the model type.</param>
    protected ReputationAssessor(
        ServiceLogger logger,
        IOptionsMonitor<DataAssessorOptions> options,
        IHttpClientFactory httpClientFactory,
        IRateQuotaService<AssessorEndpointType> rateQuotaService,
        IEndpointAddressFactory<TData> endpointAddressFactory,
        IResponseParser<TModel> responseParser)
    {
        _logger = logger;
        _options = options;
        _httpClientFactory = httpClientFactory;
        _rateQuotaService = rateQuotaService;
        _endpointAddressFactory = endpointAddressFactory;
        _responseParser = responseParser;

        _rateQuotaService.LimitExceed += OnLimitExceed;
        _rateQuotaService.LimitRecovered += OnLimitRecovered;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReputationAssessor{TData,TModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options monitor for accessing configuration settings.</param>
    /// <param name="httpClientFactory">The factory to create HTTP clients.</param>
    /// <param name="endpointAddressFactory">The factory to create endpoint addresses for the given data type.</param>
    /// <param name="responseParser">The parser to parse the HTTP response into the model type.</param>
    protected ReputationAssessor(
        ServiceLogger logger,
        IOptionsMonitor<DataAssessorOptions> options,
        IHttpClientFactory httpClientFactory,
        IEndpointAddressFactory<TData> endpointAddressFactory,
        IResponseParser<TModel> responseParser)
    {
        _logger = logger;
        _options = options;
        _httpClientFactory = httpClientFactory;
        _endpointAddressFactory = endpointAddressFactory;
        _responseParser = responseParser;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<TModel>> AssessAsync(
        TData data,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} validator service is not available to validate.");
        }

        if (!data.IsFormatValid())
        {
            throw new ArgumentException("Given data is invalid.", nameof(data));
        }

        Uri endpointAddress = _endpointAddressFactory.Create(data);

        try
        {
            HttpClient client = _httpClientFactory
                .CreateClient(_options.CurrentValue.ServiceName);
            using HttpResponseMessage response = await client.GetAsync(
                endpointAddress.PathAndQuery,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await _logger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
                return Error.Unexpected(
                    "Response.NotSuccessful",
                    "Response status code was not successful.");
            }

            _rateQuotaService?.Track(AssessorEndpointType.AssessData);
            return await _responseParser.ParseAsync(response, cancellationToken);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            if (ex.InnerException is not(null or TimeoutException))
            {
                throw;
            }

            _logger.LogWarning(ex, "A timeout exception occurred while analyzing at {ServiceName} service analyzer.", ServiceName);
            return Error.Failure(description: "Timeout exception occurred.");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ReputationAssessor{TData,TModel}"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_rateQuotaService is null)
        {
            return;
        }

        _rateQuotaService.LimitExceed -= OnLimitExceed;
        _rateQuotaService.LimitRecovered -= OnLimitRecovered;
    }

    /// <summary>
    /// Event handler for when the rate quota limit is exceeded.
    /// </summary>
    /// <param name="endpointTypes">The set of endpoint types that exceeded the rate quota limit.</param>
    /// <param name="rateQuotaPeriod">The period during which the rate quota limit was exceeded.</param>
    private void OnLimitExceed(
        HashSet<AssessorEndpointType> endpointTypes,
        RateQuotaPeriod rateQuotaPeriod)
    {
        IsAvailable = false;

        _logger.LogWarning(
            "{ServiceName}: analyzer service is not available due to a {RateQuotaPeriod} limit reached on {EndpointTypes} analysis endpoints.",
            ServiceName,
            rateQuotaPeriod,
            endpointTypes);
    }

    /// <summary>
    /// Event handler for when the rate quota limit is recovered.
    /// </summary>
    /// <param name="rateQuotaTracker">The rate quota tracker containing the endpoint types and their capacities.</param>
    private void OnLimitRecovered(
        RateQuotaTracker<AssessorEndpointType> rateQuotaTracker)
    {
        if (_rateQuotaService is null)
        {
#if DEBUG
            _logger.LogDebug("{ServiceName}: No limit tracker configured.", ServiceName);
#endif
            return;
        }

        if (!rateQuotaTracker.HasAvailableCapacity())
        {
            return;
        }

        IsAvailable = true;

#if DEBUG
        _logger.LogDebug(
            "{ServiceName}: Capacity restored for {EndpointTypes} analyses endpoints.",
            ServiceName,
            rateQuotaTracker.EndpointTypes);
#endif
    }
}