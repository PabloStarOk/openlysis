namespace Openlysis.Authentication.API.Application.Common.Configuration;

/// <summary>
/// Options for configuring refresh token behavior.
/// </summary>
internal sealed record RefreshTokenOptions
{
    /// <summary>
    /// The configuration section name for refresh token options.
    /// </summary>
    public const string SectionName = "RefreshToken";

    /// <summary>
    /// Gets or sets number of days until the refresh token expires.
    /// </summary>
    required public int ExpirationDays { get; set; }
}