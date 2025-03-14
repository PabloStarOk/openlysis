using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Configuration;
using Openlysis.Analyzers.Contracts.Enums;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.Contracts.Interfaces;

/// <summary>
/// Abstract base class for analyzers.
/// </summary>
/// <typeparam name="TAnalysis">The type of the analysis result.</typeparam>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public abstract class Analyzer<TAnalysis, TRequest> : IDisposable
     where TAnalysis : notnull
     where TRequest : notnull
{
    protected readonly ILogger<Analyzer<TAnalysis, TRequest>> _logger;
    protected readonly IOptionsMonitor<AnalyzerOptions> _options;
    protected readonly HttpClient _httpClient;

    private readonly IRequestLimitManager _requestLimitManager;
    private readonly HashSet<RequestLimitPeriod> _limitPeriodsReached = new (Enum.GetValues<RequestLimitPeriod>().Length);

    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName => _options.CurrentValue.ServiceName;
    
    /// <summary>
    /// Gets or sets a value indicating whether the service is available.
    /// </summary>
    public bool IsAvailable { get; protected set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Analyzer{TAnalysis, TRequest}"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="AnalyzerOptions"/>.</param>
    /// <param name="requestLimitManager">The request limit manager.</param>
    /// <param name="httpClient">The HTTP client instance.</param>
    /// <param name="logger">The logger instance.</param>
    protected Analyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IRequestLimitManager requestLimitManager,
        HttpClient httpClient,
        ILogger<Analyzer<TAnalysis, TRequest>> logger)
    {
        _options = options;
        _requestLimitManager = requestLimitManager;
        _httpClient = httpClient;
        _logger = logger;

        _requestLimitManager.OnLimitReached += OnLimitReached;
        _requestLimitManager.OnRateReduced += OnRateReduced;
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        _requestLimitManager.OnLimitReached -= OnLimitReached;
        _requestLimitManager.OnRateReduced -= OnRateReduced;
    }

    /// <summary>
    /// Analyzes the given request asynchronously.
    /// </summary>
    /// <param name="request">The request to analyze.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    public async Task<ErrorOr<TAnalysis>> AnalyzeAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available.");
        }

        return await OnAnalyzeAsync(request, cancellationToken);
    }

    /// <summary>
    /// Updates the status of an analysis.
    /// </summary>
    /// <param name="id">The identifier of the analysis to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated analysis status or an error.</returns>
    public async Task<ErrorOr<AnalysisStatus>> GetStatusAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available.");
        }
        
        return await OnGetStatusAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets the analysis by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the analysis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    public async Task<ErrorOr<TAnalysis>> GetAnalysisAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            return Error.Failure(description: $"{ServiceName} analysis service is not available.");
        }

        return await OnGetAnalysisAsync(id, cancellationToken);
    }
    
    /// <summary>
    /// Analyzes the given request asynchronously.
    /// </summary>
    /// <param name="request">The request to analyze.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    protected abstract Task<ErrorOr<TAnalysis>> OnAnalyzeAsync(TRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks the status of an analysis asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the analysis to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis status or an error.</returns>
    protected abstract Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the analysis by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the analysis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result or an error.</returns>
    protected abstract Task<ErrorOr<TAnalysis>> OnGetAnalysisAsync(string id, CancellationToken cancellationToken = default);
    
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