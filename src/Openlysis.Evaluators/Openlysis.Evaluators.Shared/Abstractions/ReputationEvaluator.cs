using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Requests;
using Openlysis.Application.Phones.Interfaces;
using Openlysis.Evaluators.Shared.Configuration;
using Openlysis.Evaluators.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.Logging.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.RateQuota.Models;

namespace Openlysis.Evaluators.Shared.Abstractions;

/// <summary>
/// Provides an abstract base class for evaluating the reputation of a <see cref="TRequest"/> model
/// and returning a result of type <see cref="TModel"/>.
/// </summary>
/// <typeparam name="TRequest">An implementation of <see cref="EvaluateReputationRequest"/>.</typeparam>
/// <typeparam name="TModel">The type of model to be returned with the results of the evaluation.</typeparam>
public abstract class ReputationEvaluator<TRequest, TModel>
    : IReputationEvaluator<TRequest, TModel>, IDisposable
    where TRequest : EvaluateReputationRequest
    where TModel : notnull
{
    /// <inheritdoc/>
    public string ServiceName => _options.CurrentValue.ServiceName;

    /// <inheritdoc/>
    public bool IsAvailable { get; private set; } = true;

    private readonly IServiceLogger<ReputationEvaluator<TRequest, TModel>> _logger;
    private readonly IOptionsMonitor<ReputationEvaluatorOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRateQuotaService<ReputationEndpointType>? _rateQuotaService;
    private readonly IEndpointAddressFactory<TRequest> _endpointAddressFactory;
    private readonly IResponseParser<TModel> _responseParser;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReputationEvaluator{TData,TModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options monitor for accessing configuration settings.</param>
    /// <param name="httpClientFactory">The factory to create HTTP clients.</param>
    /// <param name="rateQuotaService">The service to manage rate quotas for endpoints of the external service.</param>
    /// <param name="endpointAddressFactory">The factory to create endpoint addresses for the given data type.</param>
    /// <param name="responseParser">The parser to parse the HTTP response into the model type.</param>
    protected ReputationEvaluator(
        IServiceLogger<ReputationEvaluator<TRequest, TModel>> logger,
        IOptionsMonitor<ReputationEvaluatorOptions> options,
        IHttpClientFactory httpClientFactory,
        IRateQuotaService<ReputationEndpointType> rateQuotaService,
        IEndpointAddressFactory<TRequest> endpointAddressFactory,
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
    /// Initializes a new instance of the <see cref="ReputationEvaluator{TData,TModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options monitor for accessing configuration settings.</param>
    /// <param name="httpClientFactory">The factory to create HTTP clients.</param>
    /// <param name="endpointAddressFactory">The factory to create endpoint addresses for the given data type.</param>
    /// <param name="responseParser">The parser to parse the HTTP response into the model type.</param>
    protected ReputationEvaluator(
        IServiceLogger<ReputationEvaluator<TRequest, TModel>> logger,
        IOptionsMonitor<ReputationEvaluatorOptions> options,
        IHttpClientFactory httpClientFactory,
        IEndpointAddressFactory<TRequest> endpointAddressFactory,
        IResponseParser<TModel> responseParser)
    {
        _logger = logger;
        _options = options;
        _httpClientFactory = httpClientFactory;
        _endpointAddressFactory = endpointAddressFactory;
        _responseParser = responseParser;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<TModel>> EvaluateAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} validator service is not available to validate.");
        }

        if (!request.IsFormatValid())
        {
            throw new ArgumentException("Given request has data with invalid format.", nameof(request));
        }

        Uri endpointAddress = _endpointAddressFactory.Create(request);

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

            _rateQuotaService?.Track(ReputationEndpointType.EvaluateReputation);
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
    /// Releases the unmanaged resources used by the <see cref="ReputationEvaluator{TData,TModel}"/> and optionally releases the managed resources.
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
        HashSet<ReputationEndpointType> endpointTypes,
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
        RateQuotaTracker<ReputationEndpointType> rateQuotaTracker)
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