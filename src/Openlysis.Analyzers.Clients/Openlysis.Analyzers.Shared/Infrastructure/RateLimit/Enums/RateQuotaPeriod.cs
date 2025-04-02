namespace Openlysis.Analyzers.Shared.Infrastructure.RateLimit.Enums;

/// <summary>
/// Specifies the time period type for rate limits and usage quotas.
/// </summary>
public enum RateQuotaPeriod
{
    /// <summary>
    /// Applies to rate limits (per-minute constraints)
    /// </summary>
    Minute,

    /// <summary>
    /// Applies to rate limits (per-hour constraints).
    /// </summary>
    Hour,

    /// <summary>
    /// Applies to daily usage quotas
    /// </summary>
    Day,

    /// <summary>
    /// Applies to monthly usage quotas
    /// </summary>
    Month,
}