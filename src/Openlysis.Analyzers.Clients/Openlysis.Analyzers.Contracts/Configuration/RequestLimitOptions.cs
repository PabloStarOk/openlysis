namespace Openlysis.Analyzers.Contracts.Configuration;

/// <summary>
/// Represents the configuration options for the analyzer.
/// </summary>
/// <param name="RatePerMinute">The maximum number of requests allowed per minute.</param>
/// <param name="RatePerHour">The maximum number of requests allowed per hour.</param>
/// <param name="QuotaPerDay">The maximum number of requests allowed per day.</param>
/// <param name="QuotaPerMonth">The maximum number of requests allowed per month.</param>
/// <param name="UpdateFrequencyMs">The frequency in milliseconds at which the request limits are updated.</param>
public record RequestLimitOptions(
    int RatePerMinute,
    int RatePerHour,
    int QuotaPerDay,
    int QuotaPerMonth,
    int UpdateFrequencyMs)
{
    /// <summary>
    /// The section name for the request limit options in the configuration.
    /// </summary>
    public const string SectionName = "RequestLimitOptions";
}