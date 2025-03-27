using System.ComponentModel.DataAnnotations;

using Openlysis.Analyzers.URLQuery.Core.Models.Objects;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Options to calculate <see cref="Verdict"/> from <see cref="Sensors"/>.
/// </summary>
public record VerdictCalculationOptions
{
    /// <summary>
    /// The configuration section name for VerdictCalculationOptions.
    /// </summary>
    public const string SectionName = "UrlQuery:VerdictCalculation";

    /// <summary>
    /// Gets the threshold count of malicious verdicts to determine a verdict as malicious.
    /// </summary>
    [Required]
    required public int MaliciousVerdictCountThreshold { get; init; }

    /// <summary>
    /// Gets the threshold count of suspicious verdicts to determine a verdict as suspicious.
    /// </summary>
    [Required]
    required public int SuspiciousVerdictCountThreshold { get; init; }

    /// <summary>
    /// Gets the threshold count of malicious alert messages to determine a verdict as malicious.
    /// </summary>
    [Required]
    required public int MaliciousAlertMessageThreshold { get; init; }

    /// <summary>
    /// Gets the threshold count of suspicious alert messages to determine a verdict as suspicious.
    /// </summary>
    [Required]
    required public int SuspiciousAlertMessageThreshold { get; init; }

    /// <summary>
    /// Gets the weights for different types of sensors.
    /// </summary>
    [Required]
    required public SensorWeightsOptions SensorWeights { get; init; }

    /// <summary>
    /// Gets the alert verdicts options.
    /// </summary>
    [Required]
    required public AlertVerdictsOptions AlertVerdicts { get; init; }

    /// <summary>
    /// Gets the alert messages options.
    /// </summary>
    [Required]
    required public AlertMessagesOptions AlertMessages { get; init; }

    /// <summary>
    /// Gets the severity thresholds for IDS.
    /// </summary>
    [Required]
    required public AlertSeverityThresholds AlertSeverityThresholds { get; init; }

    /// <summary>
    /// Gets the alert count thresholds for TDS.
    /// </summary>
    [Required]
    required public AlertCountThresholdsOptions TdsAlertThresholds { get; init; }

    /// <summary>
    /// Gets the alert count thresholds for URL query.
    /// </summary>
    [Required]
    required public AlertCountThresholdsOptions UrlqueryAlertThresholds { get; init; }

    /// <summary>
    /// Normalizes a string provided by an <see cref="Alert"/> by removing hyphens, underscores, spaces, and trimming whitespace.
    /// Converts the string to lowercase.
    /// </summary>
    /// <param name="source">The source string to normalize.</param>
    /// <returns>The normalized string.</returns>
    public static string NormalizeAlertString(string source)
    {
        return source
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .Trim()
            .ToLower();
    }
}