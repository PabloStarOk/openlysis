using System.ComponentModel.DataAnnotations;

namespace Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Configuration;

/// <summary>
/// Represents the configuration options for the analyzer.
/// </summary>
public record RateQuotaEndpointOptions<TEnum>
    where TEnum : Enum
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
    required public HashSet<TEnum> EndpointTypes { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per minute.
    /// </summary>
    [Range(0, int.MaxValue)]
    required public int MinuteRate { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per hour.
    /// </summary>
    [Range(0, int.MaxValue)]
    required public int HourlyRate { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per day.
    /// </summary>
    [Range(0, int.MaxValue)]
    required public int DailyQuota { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per month.
    /// </summary>
    [Range(0, int.MaxValue)]
    required public int MonthlyQuota { get; init; }
}
