namespace Openlysis.Authentication.API.Infrastructure.Configuration;

/// <summary>
/// Options for configuring JWT generation.
/// </summary>
internal sealed record JwtGeneratorOptions
{
    /// <summary>
    /// The configuration section name for JWT generation options.
    /// </summary>
    public const string SectionName = "JwtGeneration";

    /// <summary>
    /// Gets or sets the issuer of the JWT.
    /// </summary>
    required public string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the audience for the JWT.
    /// </summary>
    required public string Audience { get; set; }

    /// <summary>
    /// Gets or sets the expiration time in seconds for the JWT.
    /// </summary>
    required public int ExpirationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the size in bytes to use for generating refresh tokens.
    /// </summary>
    required public int RefreshTokenSizeBytes { get; set; }
}
