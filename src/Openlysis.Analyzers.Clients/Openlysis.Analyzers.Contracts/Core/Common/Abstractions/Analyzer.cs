using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Models;
using Openlysis.Analyzers.Contracts.Core.Configuration;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;
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
    /// Gets a value indicating whether the service is available.
    /// </summary>
    public bool IsAvailable { get; private set; } = true;

    protected readonly ILogger<Analyzer<TAnalysis, TRequest>> _logger;
    protected readonly IOptionsMonitor<AnalyzerOptions> _options;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRequestLimitTracker? _requestLimitTracker;
    private readonly HashSet<RequestLimitPeriod> _limitPeriodsReached = new (Enum.GetValues<RequestLimitPeriod>().Length);

    /// <summary>
    /// Initializes a new instance of the <see cref="Analyzer{TAnalysis, TRequest}"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="AnalyzerOptions"/>.</param>
    /// <param name="requestLimitTracker">The request limit manager.</param>
    /// <param name="httpClientFactory">The HTTP client factory instance.</param>
    /// <param name="logger">The logger instance.</param>
    protected Analyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IRequestLimitTracker requestLimitTracker,
        IHttpClientFactory httpClientFactory,
        ILogger<Analyzer<TAnalysis, TRequest>> logger)
    {
        _options = options;
        _requestLimitTracker = requestLimitTracker;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _requestLimitTracker.OnLimitReached += OnLimitReached;
        _requestLimitTracker.OnRateReduced += OnRateReduced;
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

        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available.");
        }

        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(ServiceName);
            ErrorOr<TAnalysis> result = await OnAnalyzeAsync(httpClient, request, cancellationToken);
            if (!result.IsError && _options.CurrentValue.AnalyzeConsumeRequest)
            {
                _requestLimitTracker?.AddRequest();
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

        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available.");
        }

        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(ServiceName);
            ErrorOr<AnalysisStatus> result = await OnGetStatusAsync(httpClient, id, cancellationToken);
            if (!result.IsError && _options.CurrentValue.GetStatusConsumeRequest)
            {
                _requestLimitTracker?.AddRequest();
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

        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available.");
        }

        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(ServiceName);
            ErrorOr<TAnalysis> result = await OnGetAnalysisAsync(httpClient, id, cancellationToken);
            if (!result.IsError && _options.CurrentValue.GetAnalysisConsumeRequest)
            {
                _requestLimitTracker?.AddRequest();
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
        if (_requestLimitTracker is null)
        {
            return;
        }

        _requestLimitTracker.OnLimitReached -= OnLimitReached;
        _requestLimitTracker.OnRateReduced -= OnRateReduced;
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
    /// Handles the event when a request limit is reached.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="limitPeriod">The period for which the limit was reached.</param>
    private void OnLimitReached(object? sender, RequestLimitPeriod limitPeriod)
    {
        if (!_limitPeriodsReached.Add(limitPeriod))
        {
            return;
        }

        IsAvailable = false;
        _logger.LogWarning(
            "{ServiceName} analyzer service is not available due to a {LimitPeriod} limit reached.",
            ServiceName,
            limitPeriod);
    }

    /// <summary>
    /// Handles the event when the rate limit is reduced.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="limitPeriod">The period for which the limit was reduced.</param>
    private void OnRateReduced(object? sender, RequestLimitPeriod limitPeriod)
    {
        _limitPeriodsReached.Remove(limitPeriod);

        if (_limitPeriodsReached.Count == 0)
        {
            IsAvailable = true;
        }
    }
}