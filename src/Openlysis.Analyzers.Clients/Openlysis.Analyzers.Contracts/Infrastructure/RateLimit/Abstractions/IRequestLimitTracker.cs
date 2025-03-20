using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;

namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;

/// <summary>
/// Define a manager of request limits.
/// </summary>
public interface IRequestLimitTracker
{
    /// <summary>
    /// Event triggered when the request limit is reached.
    /// This can occur for any <see cref="RequestLimitPeriod"/>.
    /// </summary>
    public event EventHandler<RequestLimitPeriod> OnLimitReached;

    /// <summary>
    /// Event triggered when the request limit is cleared.
    /// This can occur for any <see cref="RequestLimitPeriod"/>.
    /// </summary>
    /// <remarks>
    /// This event is useful for monitoring and handling scenarios where the request limit has been reduced or reset.
    /// </remarks>
    public event EventHandler<RequestLimitPeriod> OnRateReduced;

    /// <summary>
    /// Adds a new request that was made to the limit count.
    /// </summary>
    public void AddRequest();

    /// <summary>
    /// Resets the daily request count.
    /// </summary>
    internal void ResetDailyRequestCount();

    /// <summary>
    /// Resets the monthly request count.
    /// </summary>
    internal void ResetMonthlyRequestCount();
}