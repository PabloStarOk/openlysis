using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Models;
using Openlysis.Analyzers.Contracts.Core.Configuration;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Models;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;

namespace Openlysis.Analyzers.Contracts.Core.Common.Abstractions;

/// <summary>
/// Abstract base class for analyzers.
/// </summary>
/// <typeparam name="TAnalysis">The type of the analysis result.</typeparam>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public abstract class Analyzer<TAnalysis, TRequest> : IDisposable
     where TAnalysis : notnull
     where TRequest : AnalyzeRequest
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName => _options.CurrentValue.ServiceName;

    /// <summary>
    /// Gets a value indicating whether the analyzer can perform analysis.
    /// </summary>
    public bool CanAnalyze { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether the analyzer can get the status of an analysis.
    /// </summary>
    public bool CanGetAnalysisStatus { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether the analyzer can get the analysis.
    /// </summary>
    public bool CanGetAnalysis { get; private set; } = true;

    /// <summary>
    /// Logger instance for the analyzer.
    /// </summary>
    protected readonly ILogger<Analyzer<TAnalysis, TRequest>> _logger;

    /// <summary>
    /// Options monitor for <see cref="AnalyzerOptions"/>.
    /// </summary>
    protected readonly IOptionsMonitor<AnalyzerOptions> _options;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRateQuotaService? _rateQuotaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Analyzer{TAnalysis, TRequest}"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="AnalyzerOptions"/>.</param>
    /// <param name="rateQuotaService">The request limit manager.</param>
    /// <param name="httpClientFactory">The HTTP client factory instance.</param>
    /// <param name="logger">The logger instance.</param>
    protected Analyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IRateQuotaService rateQuotaService,
        IHttpClientFactory httpClientFactory,
        ILogger<Analyzer<TAnalysis, TRequest>> logger)
    {
        _options = options;
        _rateQuotaService = rateQuotaService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _rateQuotaService.LimitExceed += OnCapacityExhausted;
        _rateQuotaService.LimitRecovered += OnCapacityRestored;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Analyzer{TAnalysis, TRequest}"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="AnalyzerOptions"/>.</param>
    /// <param name="httpClientFactory">The HTTP client factory instance.</param>
    /// <param name="logger">The logger instance.</param>
    protected Analyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<Analyzer<TAnalysis, TRequest>> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Analyzes the given request asynchronously.
    /// </summary>
    /// <param name="request">The request to analyze.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    public async Task<ErrorOr<TAnalysis>> AnalyzeAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!CanAnalyze)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available to analyze.");
        }

        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(ServiceName);
            ErrorOr<TAnalysis> result = await OnAnalyzeAsync(httpClient, request, cancellationToken);
            if (!result.IsError)
            {
                _rateQuotaService?.Track(AnalysisEndpointType.Analyze);
            }

            return result;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            if (ex.InnerException is not(null or TimeoutException))
            {
                throw;
            }

            _logger.LogWarning(ex, "A timeout exception occurred while analyzing at {ServiceName} service analyzer.", ServiceName);
            return Error.Failure("Timeout exception occurred.");
        }
    }

    /// <summary>
    /// Updates the status of an analysis.
    /// </summary>
    /// <param name="id">A <see cref="ComposedServiceAnalysisId"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated analysis status or an error.</returns>
    public async Task<ErrorOr<AnalysisStatus>> GetStatusAsync(
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id.Primary.Value);

        if (!CanGetAnalysisStatus)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available to get the status of an analysis.");
        }

        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(ServiceName);
            ErrorOr<AnalysisStatus> result = await OnGetStatusAsync(httpClient, id, cancellationToken);
            if (!result.IsError)
            {
                _rateQuotaService?.Track(AnalysisEndpointType.GetStatus);
            }

            return result;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            if (ex.InnerException is not(null or TimeoutException))
            {
                throw;
            }

            _logger.LogWarning(ex, "A timeout exception occurred while analyzing at {ServiceName} service analyzer.", ServiceName);
            return Error.Failure("Timeout exception occurred.");
        }
    }

    /// <summary>
    /// Gets the analysis by its identifier asynchronously.
    /// </summary>
    /// <param name="id">A <see cref="ComposedServiceAnalysisId"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    public async Task<ErrorOr<TAnalysis>> GetAnalysisAsync(
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id.Primary.Value);

        if (!CanGetAnalysis)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available to get an analysis.");
        }

        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(ServiceName);
            ErrorOr<TAnalysis> result = await OnGetAnalysisAsync(httpClient, id, cancellationToken);
            if (!result.IsError)
            {
                _rateQuotaService?.Track(AnalysisEndpointType.GetResults);
            }

            return result;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            if (ex.InnerException is not(null or TimeoutException))
            {
                throw;
            }

            _logger.LogWarning(ex, "A timeout exception occurred while analyzing at {ServiceName} service analyzer.", ServiceName);
            return Error.Failure("Timeout exception occurred.");
        }
    }

    /// <summary>
    /// Disposes the resources used by the analyzer.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method is called from the Dispose method.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_rateQuotaService is null)
        {
            return;
        }

        _rateQuotaService.LimitExceed -= OnCapacityExhausted;
        _rateQuotaService.LimitRecovered -= OnCapacityRestored;
    }

    /// <summary>
    /// Analyzes the given request asynchronously.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the analysis.</param>
    /// <param name="request">The request to analyze.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    protected abstract Task<ErrorOr<TAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        TRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the status of an analysis asynchronously.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for checking the status.</param>
    /// <param name="id">The identifier of the analysis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis status or an error.</returns>
    protected abstract Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the analysis by its identifier asynchronously.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for getting the analysis.</param>
    /// <param name="id">The identifier of the analysis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    protected abstract Task<ErrorOr<TAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles the event when the request capacity is exhausted.
    /// </summary>
    /// <param name="endpointTypes">The set of analysis endpoint types that have exhausted their capacity.</param>
    /// <param name="rateQuotaPeriod">The rate quota period during which the capacity was exhausted.</param>
    private void OnCapacityExhausted(
        HashSet<AnalysisEndpointType> endpointTypes,
        RateQuotaPeriod rateQuotaPeriod)
    {
        UpdateAvailability(endpointTypes, false);

        _logger.LogWarning(
            "{ServiceName}: analyzer service is not available due to a {RateQuotaPeriod} limit reached on {EndpointTypes} analysis endpoints.",
            ServiceName,
            rateQuotaPeriod,
            endpointTypes);
    }

    /// <summary>
    /// Handles the event when the request capacity is restored.
    /// </summary>
    /// <param name="tracker">The rate quota tracker that indicates the restored capacity.</param>
    private void OnCapacityRestored(RateQuotaTracker tracker)
    {
        if (_rateQuotaService is null)
        {
#if DEBUG
            _logger.LogDebug("{ServiceName}: No limit tracker configured.", ServiceName);
#endif
            return;
        }

        if (_rateQuotaService.AreAvailable(tracker.EndpointTypes))
        {
            UpdateAvailability(tracker.EndpointTypes, true);
        }

#if DEBUG
        _logger.LogDebug(
            "{ServiceName}: Capacity restored for {EndpointTypes} analyses endpoints.",
            ServiceName,
            tracker.EndpointTypes);
#endif
    }

    /// <summary>
    /// Updates the availability status of the specified analysis endpoint types.
    /// </summary>
    /// <param name="endpointTypes">The set of analysis endpoint types to update.</param>
    /// <param name="value">The availability status to set.</param>
    private void UpdateAvailability(
        HashSet<AnalysisEndpointType> endpointTypes,
        bool value)
    {
        foreach (var endpointType in endpointTypes)
        {
            switch (endpointType)
            {
                case AnalysisEndpointType.Analyze:
                    CanAnalyze = value;
                    break;

                case AnalysisEndpointType.GetStatus:
                    CanGetAnalysisStatus = value;
                    break;

                case AnalysisEndpointType.GetResults:
                    CanGetAnalysis = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(endpointTypes), endpointTypes, null);
            }
        }
    }
}