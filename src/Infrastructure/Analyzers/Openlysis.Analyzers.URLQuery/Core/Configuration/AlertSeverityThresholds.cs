using System.ComponentModel.DataAnnotations;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the severity thresholds for alerts.
/// </summary>
public record AlertSeverityThresholds
{
    /// <summary>
    /// Gets the threshold of alerts with a high severity to determine as malicious.
    /// </summary>
    [Required]
    required public int HighSeverityMaliciousThreshold { get; init; }

    /// <summary>
    /// Gets the threshold of alerts with a medium severity to determine as suspicious.
    /// </summary>
    [Required]
    required public int MediumSeveritySuspiciousThreshold { get; init; }
}