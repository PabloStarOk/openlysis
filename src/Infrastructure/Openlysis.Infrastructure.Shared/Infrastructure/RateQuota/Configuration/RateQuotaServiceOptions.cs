using System.ComponentModel.DataAnnotations;

using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Services;

namespace Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Configuration;

/// <summary>
/// Options to configure a <see cref="RateQuotaService{TEnum}"/>.
/// </summary>
public record RateQuotaServiceOptions
{
    /// <summary>
    /// The section name for the request limit options in the configuration.
    /// </summary>
    public const string SectionName = "RateQuotaService";

    /// <summary>
    /// Gets the frequency in milliseconds at which the request limits are updated.
    /// </summary>]
    [Range(1, int.MaxValue)]
    required public int RateWindowRefreshIntervalMs { get; init; }
}