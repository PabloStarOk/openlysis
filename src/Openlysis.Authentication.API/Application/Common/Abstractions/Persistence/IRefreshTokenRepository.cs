using ErrorOr;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;

/// <summary>
/// Repository abstraction for managing refresh tokens in persistence.
/// </summary>
internal interface IRefreshTokenRepository
{
    /// <summary>
    /// Adds a new refresh token to the repository.
    /// </summary>
    /// <param name="token">The refresh token to add.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddAsync(RefreshToken token);

    /// <summary>
    /// Revokes an existing refresh token and adds a new one atomically.
    /// </summary>
    /// <param name="revokedToken">The token to revoke.</param>
    /// <param name="newToken">The new token to add.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or error.</returns>
    public Task<ErrorOr<Success>> RevokeAndAddAsync(
        RefreshToken revokedToken,
        RefreshToken newToken);

    /// <summary>
    /// Revokes all refresh tokens associated with a specific user.
    /// </summary>
    /// <param name="userId">The global ID of the user.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task RevokeAllForUserAsync(GlobalId userId);

    /// <summary>
    /// Retrieves a refresh token by its hash value.
    /// </summary>
    /// <param name="hash">The hash of the refresh token.</param>
    /// <returns>The matching <see cref="RefreshToken"/> if found; otherwise, null.</returns>
    public Task<RefreshToken?> GetByHashAsync(byte[] hash);
}