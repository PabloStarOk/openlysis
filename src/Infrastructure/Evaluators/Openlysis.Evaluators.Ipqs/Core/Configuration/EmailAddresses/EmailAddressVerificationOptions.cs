using System.ComponentModel.DataAnnotations;

namespace Openlysis.Evaluators.Ipqs.Core.Configuration.EmailAddresses;

/// <summary>
/// Represents the configuration options for email address verification.
/// </summary>
public record EmailAddressVerificationOptions
{
    /// <summary>
    /// The configuration section name for email address verification settings.
    /// </summary>
    public const string SectionName = "Ipqs:EmailAddressVerification";

    /// <summary>
    /// Gets or initializes the timeout (in seconds) to verify an email address.
    /// The value must be between 1 and 60 seconds.
    /// </summary>
    [Required]
    [Range(1, 60, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public int Timeout { get; init; } = 7;
}