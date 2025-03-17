namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Configuration;

/// <summary>
/// Represents the configuration options for the analyzer.
/// </summary>
public record RequestLimitOptions
{
    /// <summary>
    /// The section name for the request limit options in the configuration.
    /// </summary>
    public const string SectionName = "RequestLimitOptions";
    
    /// <summary>
    /// Gets the maximum number of requests allowed per minute.
    /// </summary>
    public int RatePerMinute { get; init; }
    
    /// <summary>
    /// Gets the maximum number of requests allowed per hour.
    /// </summary>
    public int RatePerHour { get; init; }
    
    /// <summary>
    /// Gets the maximum number of requests allowed per day.
    /// </summary>
    public int QuotaPerDay { get; init; }
    
    /// <summary>
    /// Gets the maximum number of requests allowed per month.
    /// </summary>
    public int QuotaPerMonth { get; init; }
    
    /// <summary>
    /// Gets the frequency in milliseconds at which the request limits are updated.
    /// </summary>
    public int UpdateFrequencyMs { get; init; }
}