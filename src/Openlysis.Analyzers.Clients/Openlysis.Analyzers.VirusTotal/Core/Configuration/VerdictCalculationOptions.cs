using System.ComponentModel.DataAnnotations;

using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

namespace Openlysis.Analyzers.VirusTotal.Core.Configuration;

/// <summary>
/// Options for calculating verdicts based on thresholds.
/// </summary>
public record VerdictCalculationOptions
{
    /// <summary>
    /// The configuration section name for verdict calculation options.
    /// </summary>
    public const string SectionName = "VirusTotal:VerdictCalculation";

    /// <summary>
    /// Gets the maximum number of timeout detections allowed for marking a verdict as unknown.
    /// </summary>
    /// <value>
    /// The maximum allowed timeout percentage.
    /// </value>
    [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public float MaxAllowedTimeoutPercentage { get; init; }

    /// <summary>
    /// Gets the threshold for marking a verdict as undetected for harmless/undetected of <see cref="Stats"/>.
    /// </summary>
    /// <value>
    /// The threshold percentage for an undetected verdict.
    /// </value>
    [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public float UndetectedThresholdPercentage { get; init; }

    /// <summary>
    /// Gets the threshold for marking a verdict as suspicious.
    /// </summary>
    /// <value>
    /// The threshold percentage for a suspicious verdict.
    /// </value>
    [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public float SuspiciousThresholdPercentage { get; init;  }

    /// <summary>
    /// Gets the threshold for marking a verdict as malicious.
    /// </summary>
    /// <value>
    /// The threshold percentage for a malicious verdict.
    /// </value>
    [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public float MaliciousThresholdPercentage { get; init; }
}