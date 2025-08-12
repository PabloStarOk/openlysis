namespace Openlysis.API.Configuration.Options;

/// <summary>
/// Options for configuring the authentication signing key.
/// </summary>
internal sealed record AuthSigningKeyOptions
{
    /// <summary>
    /// The configuration section name for the signing key options.
    /// </summary>
    public const string SectionName = "AuthSigningKey";

    /// <summary>
    /// Gets the PEM-encoded signing key used for authentication.
    /// </summary>
    required public string SigningKeyPem { get; init; }
}