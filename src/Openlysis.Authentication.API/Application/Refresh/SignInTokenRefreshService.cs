using ErrorOr;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Application.Common.Models;
using Openlysis.Authentication.API.Application.Common.Services;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Application.Refresh;

/// <summary>
/// Service responsible for refreshing sign-in tokens.
/// </summary>
internal sealed class SignInTokenRefreshService : ISignInTokenRefreshService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly RefreshTokenStore _refreshTokenStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignInTokenRefreshService"/> class.
    /// </summary>
    /// <param name="userRepository">Repository for user data access.</param>
    /// <param name="tokenGenerator">Service for generating authentication tokens.</param>
    /// <param name="refreshTokenStore">Store for managing refresh tokens.</param>
    public SignInTokenRefreshService(
        IUserRepository userRepository,
        ITokenGenerator tokenGenerator,
        RefreshTokenStore refreshTokenStore)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<AuthTokens>> RefreshAsync(string refreshToken)
    {
        RefreshToken? oldRefreshToken =
            await _refreshTokenStore.GetAsync(refreshToken);
        if (oldRefreshToken is null ||
            DateTimeOffset.UtcNow >= oldRefreshToken.ExpiresAt)
        {
            return Error.Unauthorized();
        }

        User user = await _userRepository.GetByIdAsync(oldRefreshToken.UserId);

        if (oldRefreshToken.RevokedAt is not null)
        {
            await _refreshTokenStore.RevokeAllForUserAsync(user.Id);
            return Error.Unauthorized();
        }

        AuthTokens newTokens = _tokenGenerator.Generate(user);
        await _refreshTokenStore.RotateAsync(
            user.Id,
            oldRefreshToken,
            newTokens.RefreshToken);

        return newTokens;
    }
}