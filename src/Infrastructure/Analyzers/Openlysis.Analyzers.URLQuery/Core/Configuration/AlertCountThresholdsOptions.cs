using System.ComponentModel.DataAnnotations;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the alert count thresholds for the analyzer.
/// </summary>
public record AlertCountThresholdsOptions
{
    /// <summary>
    /// Gets the threshold of malicious alerts to determine as malicious.
    /// </summary>
    [Required]
    required public int MaliciousAlertThreshold { get; init; }

    /// <summary>
    /// Gets the threshold of suspicious alerts to determine as suspicious.
    /// </summary>
    [Required]
    required public int SuspiciousAlertThreshold { get; init; }
}