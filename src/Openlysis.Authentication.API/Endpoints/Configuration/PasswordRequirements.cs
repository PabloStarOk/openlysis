namespace Openlysis.Authentication.API.Endpoints.Configuration;

/// <summary>
/// The password requirements.
/// </summary>
internal sealed record PasswordRequirements
{
    /// <summary>
    /// The configuration section name for password requirements.
    /// </summary>
    public const string SectionName = "PasswordRequirements";

    /// <summary>
    /// Gets or sets minimum required length for the password.
    /// </summary>
    public int MinLength { get; set; }

    /// <summary>
    /// Gets or sets minimum number of lowercase characters required.
    /// </summary>
    public int MinLowerChars { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of uppercase characters required.
    /// </summary>
    public int MinUpperChars { get; set; }

    /// <summary>
    /// Gets or sets minimum number of digit characters required.
    /// </summary>
    public int MinDigits { get; set; }

    /// <summary>
    /// Gets or sets minimum number of special characters required.
    /// </summary>
    public int MinSpecialChars { get; set; }
}