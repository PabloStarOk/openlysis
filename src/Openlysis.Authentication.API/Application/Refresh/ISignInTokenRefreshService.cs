using ErrorOr;

using Openlysis.Authentication.API.Application.Common.Models;

namespace Openlysis.Authentication.API.Application.Refresh;

/// <summary>
/// Service for refreshing sign-in tokens using a refresh token.
/// </summary>
internal interface ISignInTokenRefreshService
{
    /// <summary>
    /// Refreshes authentication tokens using the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use for obtaining new tokens.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an <see cref="ErrorOr{AuthTokens}"/> with the new authentication tokens or an error.
    /// </returns>
    public Task<ErrorOr<AuthTokens>> RefreshAsync(string refreshToken);
}
