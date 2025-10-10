namespace Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

/// <summary>
/// Interface for restoring quota usage limits.
/// </summary>
public interface IQuotaRestorable
{
    /// <summary>
    /// Restores the daily usage limits for all endpoints.
    /// </summary>
    internal void RestoreDailyUsage();

    /// <summary>
    /// Restores the monthly usage limits for all endpoints.
    /// </summary>
    internal void RestoreMonthlyUsage();
}