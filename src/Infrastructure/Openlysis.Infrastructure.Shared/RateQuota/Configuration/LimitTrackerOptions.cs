using System.ComponentModel.DataAnnotations;

using Openlysis.Infrastructure.Shared.RateQuota.Services;

namespace Openlysis.Infrastructure.Shared.RateQuota.Configuration;

/// <summary>
/// Options to configure a <see cref="RateQuotaService"/>.
/// </summary>
public record LimitTrackerOptions
{
    /// <summary>
    /// The section name for the request limit options in the configuration.
    /// </summary>
    public const string SectionName = "RateQuotaService";

    /// <summary>
    /// Gets the frequency in milliseconds at which the request limits are updated.
    /// </summary>]
    [Required]
    required public int RateWindowRefreshIntervalMs { get; init; }
}