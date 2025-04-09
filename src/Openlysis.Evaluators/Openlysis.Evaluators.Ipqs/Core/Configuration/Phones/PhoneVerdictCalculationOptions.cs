using System.ComponentModel.DataAnnotations;

using Openlysis.Domain.Common.Enums;

namespace Openlysis.Evaluators.Ipqs.Core.Configuration.Phones;

/// <summary>
/// Configuration options for phone verdict calculation.
/// </summary>
public record PhoneVerdictCalculationOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "Ipqs:VerdictCalculation:Phone";

    /// <summary>
    /// Gets the threshold for considering the fraud score as suspicious.
    /// </summary>
    [Required]
    [Range(0, 99, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public int FraudScoreSuspiciousThreshold { get; init; }

    /// <summary>
    /// Gets the threshold for considering the fraud score as malicious.
    /// </summary>
    [Required]
    [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
    required public int FraudScoreMaliciousThreshold { get; init; }

    /// <summary>
    /// Gets the verdict to use when a phone number is considered spam.
    /// </summary>
    [Required]
    required public Verdict SpamVerdict { get; init; }

    /// <summary>
    /// Gets the verdict to use when a phone number is a VoIP number.
    /// </summary>
    [Required]
    required public Verdict VoipVerdict { get; init; }

    /// <summary>
    /// Gets the verdict to use when a phone number is inactive.
    /// </summary>
    [Required]
    required public Verdict InactiveVerdict { get; init; }
}