using NodaTime;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Infrastructure.Persistence.Entities;

/// <summary>
/// A database model for <see cref="RefreshToken"/>.
/// </summary>
internal sealed record DbRefreshToken
{
    /// <summary>
    /// Gets the unique identifier for the refresh token.
    /// </summary>
    public long TokenId { get; init; }

    /// <summary>
    /// Gets the hashed value of the refresh token.
    /// </summary>
    required public byte[] TokenHash { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user associated with the token.
    /// </summary>
    required public Guid UserId { get; init; }

    /// <summary>
    /// Gets the creation date and time of the refresh token.
    /// </summary>
    required public Instant CreatedAt { get; init; }

    /// <summary>
    /// Gets the expiration date and time of the refresh token.
    /// </summary>
    required public Instant ExpiresAt { get; init; }

    /// <summary>
    /// Gets the date and time when the refresh token was revoked, if applicable.
    /// </summary>
    required public Instant? RevokedAt { get; init; }

    /// <summary>
    /// Converts this database entity to its corresponding domain model.
    /// </summary>
    /// <returns>A <see cref="RefreshToken"/> domain object.</returns>
    public RefreshToken ToDomainToken()
    {
        var userId = GlobalId.Parse(UserId.ToString());
        return RefreshToken.CreateWithId(
            TokenId,
            TokenHash,
            userId,
            CreatedAt.ToDateTimeOffset(),
            ExpiresAt.ToDateTimeOffset(),
            RevokedAt?.ToDateTimeOffset());
    }
}