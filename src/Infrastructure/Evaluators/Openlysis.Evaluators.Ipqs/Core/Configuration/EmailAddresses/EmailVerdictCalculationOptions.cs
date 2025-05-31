using System.ComponentModel.DataAnnotations;

namespace Openlysis.Evaluators.Ipqs.Core.Configuration.EmailAddresses;

/// <summary>
/// Represents the configuration options for calculating email verdicts,
/// including thresholds for suspicious and malicious fraud scores.
/// </summary>
public record EmailVerdictCalculationOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "Ipqs:VerdictCalculation:EmailAddress";

    /// <summary>
    /// Gets the threshold for considering the fraud score as suspicious.
    /// </summary>
    [Required]
    [Range(1, 99, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public int FraudScoreSuspiciousThreshold { get; init; }

    /// <summary>
    /// Gets the threshold for considering the fraud score as malicious.
    /// </summary>
    [Required]
    [Range(1, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public int FraudScoreMaliciousThreshold { get; init; }
}