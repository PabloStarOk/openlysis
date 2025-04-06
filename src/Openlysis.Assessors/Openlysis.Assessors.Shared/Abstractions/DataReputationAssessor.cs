using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Assessors.Shared.Configuration;
using Openlysis.Assessors.Shared.Models.Common;

namespace Openlysis.Assessors.Shared.Abstractions;

/// <summary>
/// Assess the reputation of a <see cref="TData"/> model.
/// </summary>
/// <typeparam name="TData">The type of data to be assessed.</typeparam>
/// <typeparam name="TModel">The type of model to be returned after assessment.</typeparam>
public abstract class DataReputationAssessor<TData, TModel>
    : IDataReputationAssessor<TData, TModel>
    where TData : AssessedData
    where TModel : notnull
{
    /// <inheritdoc/>
    public string ServiceName => _options.CurrentValue.ServiceName;

    /// <inheritdoc/>
    public bool IsAvailable { get; private set; }

    // TODO: Add RateQuotaService.
    // TODO: Add AnalyzerLogger.
    private readonly ILogger<DataReputationAssessor<TData, TModel>> _logger;
    private readonly IOptionsMonitor<DataAssessorOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEndpointAddressFactory<TData> _endpointAddressFactory;
    private readonly IResponseParser<TModel> _responseParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataReputationAssessor{TData, TModel}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options monitor for accessing configuration settings.</param>
    /// <param name="httpClientFactory">The factory to create HTTP clients.</param>
    /// <param name="endpointAddressFactory">The factory to create endpoint addresses for the given data type.</param>
    /// <param name="responseParser">The parser to parse the HTTP response into the model type.</param>
    protected DataReputationAssessor(
        ILogger<DataReputationAssessor<TData, TModel>> logger,
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
                // TODO: Log non success status code.
                return Error.Unexpected(
                    "Response.NotSuccessful",
                    "Response status code was not successful.");
            }

            // TODO: Track request.
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
}