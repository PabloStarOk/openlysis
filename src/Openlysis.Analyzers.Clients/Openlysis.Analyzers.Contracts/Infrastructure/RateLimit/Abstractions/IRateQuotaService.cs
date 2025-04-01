using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Models;

namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;

/// <summary>
/// Define a tracker of requests sent to an external service.
/// </summary>
public interface IRateQuotaService
{
    /// <summary>
    /// Event triggered when the request capacity is exhausted.
    /// </summary>
    public event Action<HashSet<AnalysisEndpointType>, RateQuotaPeriod> LimitExceed;

    /// <summary>
    /// Event triggered when the request capacity is restored.
    /// </summary>
    public event Action<RateQuotaTracker>? LimitRecovered;

    /// <summary>
    /// Tracks the request limit for a specific endpoint type.
    /// </summary>
    /// <param name="endpointType">The type of analysis endpoint to track.</param>
    public void Track(AnalysisEndpointType endpointType);

    /// <summary>
    /// Checks if the request limits are available for the specified endpoint types.
    /// </summary>
    /// <param name="endpointTypes">The set of analysis endpoint types to check.</param>
    /// <returns>True if the endpoints can receive requests.</returns>
    public bool AreAvailable(HashSet<AnalysisEndpointType> endpointTypes);

    /// <summary>
    /// Restores the daily usage limits for all endpoints.
    /// </summary>
    internal void RestoreDailyUsage();

    /// <summary>
    /// Restores the monthly usage limits for all endpoints.
    /// </summary>
    internal void RestoreMonthlyUsage();
}