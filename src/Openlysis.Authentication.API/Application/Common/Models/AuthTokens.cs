using NodaTime;

namespace Openlysis.Authentication.API.Application.Common.Models;

/// <summary>
/// A pair of authentication tokens: access and refresh tokens.
/// </summary>
/// <param name="AccessToken">The access token.</param>
/// <param name="RefreshToken">The refresh token used to obtain new access tokens.</param>
/// <param name="RefreshTokenExpiration"> The expiration timestamp (in UTC) for the refresh token.</param>
internal sealed record AuthTokens(
    string AccessToken,
    string RefreshToken,
    Instant RefreshTokenExpiration);