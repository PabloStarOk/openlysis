using System.ComponentModel.DataAnnotations;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;

namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Configuration;

/// <summary>
/// Represents the configuration options for the analyzer.
/// </summary>
public record RateQuotaOptions
{
    /// <summary>
    /// The section name for the request limit options in the configuration.
    /// </summary>
    public const string SectionName = "RateQuota";

    /// <summary>
    /// Gets a <see cref="EndpointTypes"/> definining the endpoint which the rate and quota limits are for.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "EndpointTypes doesn't contain any value, either a misspelling error or unprovided values.")]
    required public HashSet<AnalysisEndpointType> EndpointTypes { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per minute.
    /// </summary>
    [Required]
    required public int MinuteRate { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per hour.
    /// </summary>
    [Required]
    required public int HourlyRate { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per day.
    /// </summary>
    [Required]
    required public int DailyQuota { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per month.
    /// </summary>
    [Required]
    required public int MonthlyQuota { get; init; }
}
