using NodaTime;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Application.Common.Services;

/// <summary>
/// Provides operations for storing, rotating, retrieving, and revoking refresh tokens.
/// </summary>
internal sealed class RefreshTokenStore
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenHasher _tokenHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenStore"/> class.
    /// </summary>
    /// <param name="refreshTokenRepository">The repository for refresh tokens.</param>
    /// <param name="tokenHasher">The hasher for refresh tokens.</param>
    public RefreshTokenStore(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenHasher tokenHasher)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenHasher = tokenHasher;
    }

    /// <summary>
    /// Adds a new refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="encodedRefreshToken">The encoded refresh token.</param>
    /// <param name="expiration">The expiration instant for the refresh token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddAsync(
        GlobalId userId,
        string encodedRefreshToken,
        Instant expiration)
    {
        RefreshToken refreshToken =
            CreateRefreshTokenEntity(encodedRefreshToken, expiration, userId);
        await _refreshTokenRepository.AddAsync(refreshToken);
    }

    /// <summary>
    /// Rotates the refresh token for the specified user, revoking the old token and adding a new one.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="oldRefreshToken">The old refresh token to revoke.</param>
    /// <param name="newEncodedRefreshToken">The new encoded refresh token.</param>
    /// <param name="newRefreshTokenExpiration">The expiration instant for the new refresh token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RotateAsync(
        GlobalId userId,
        RefreshToken oldRefreshToken,
        string newEncodedRefreshToken,
        Instant newRefreshTokenExpiration)
    {
        RefreshToken newRefreshToken = CreateRefreshTokenEntity(
                newEncodedRefreshToken,
                newRefreshTokenExpiration,
                userId);

        oldRefreshToken.Revoke();

        await _refreshTokenRepository.RevokeAndAddAsync(
            oldRefreshToken,
            newRefreshToken);
    }

    /// <summary>
    /// Retrieves a refresh token entity by its encoded value.
    /// </summary>
    /// <param name="refreshToken">The encoded refresh token.</param>
    /// <returns>The matching <see cref="RefreshToken"/> if found; otherwise, null.</returns>
    public async Task<RefreshToken?> GetAsync(string refreshToken)
    {
        var tokenHash = _tokenHasher.Hash(refreshToken);
        return await _refreshTokenRepository.GetByHashAsync(tokenHash);
    }

    /// <summary>
    /// Revokes all refresh tokens for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RevokeAllForUserAsync(GlobalId userId)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
    }

    /// <summary>
    /// Creates a new <see cref="RefreshToken"/> entity for the specified user and token.
    /// </summary>
    /// <param name="refreshToken">The encoded refresh token.</param>
    /// <param name="expiration">The expiration instant for the refresh token.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new <see cref="RefreshToken"/> entity.</returns>
    private RefreshToken CreateRefreshTokenEntity(
        string refreshToken,
        Instant expiration,
        GlobalId userId)
    {
        byte[] tokenHash = _tokenHasher.Hash(refreshToken);
        return RefreshToken.Create(tokenHash, userId, expiration.ToDateTimeOffset());
    }
}