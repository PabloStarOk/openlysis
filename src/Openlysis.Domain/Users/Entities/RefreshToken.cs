using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Users.Entities;

/// <summary>
/// Represents a refresh token entity used for user authentication and session management.
/// </summary>
public sealed class RefreshToken : Entity<long>
{
    /// <summary>
    /// Gets the hashed value of the refresh token.
    /// </summary>
    public byte[] TokenHash { get; }

    /// <summary>
    /// Gets the unique identifier of the user associated with this refresh token.
    /// </summary>
    public GlobalId UserId { get; }

    /// <summary>
    /// Gets the timestamp when the refresh token was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the timestamp when the refresh token expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>
    /// Gets the timestamp when the refresh token was revoked, or null if it has not been revoked.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the refresh token entity.</param>
    /// <param name="tokenHash">The hashed value of the refresh token.</param>
    /// <param name="userId">The unique identifier of the user associated with this refresh token.</param>
    /// <param name="createdAt">The timestamp when the refresh token was created.</param>
    /// <param name="expiresAt">The timestamp when the refresh token expires.</param>
    /// <param name="revokedAt">The timestamp when the refresh token was revoked, or null if not revoked.</param>
    private RefreshToken(
        long id,
        byte[] tokenHash,
        GlobalId userId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt)
        : base(id)
    {
        RevokedAt = revokedAt;
        TokenHash = tokenHash;
        UserId = userId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Creates a new <see cref="RefreshToken"/> instance.
    /// </summary>
    /// <param name="tokenHash">The hashed value of the refresh token.</param>
    /// <param name="userId">The unique identifier of the user associated with this refresh token.</param>
    /// <param name="expirationDays">The number of days until the token expires.</param>
    /// <returns>A new <see cref="RefreshToken"/> instance.</returns>
    public static RefreshToken Create(
        byte[] tokenHash,
        GlobalId userId,
        int expirationDays)
    {
        long randomId = Random.Shared.NextInt64();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expiresAt = now.AddDays(expirationDays);
        return new RefreshToken(
            randomId,
            tokenHash,
            userId,
            createdAt: now,
            expiresAt,
            revokedAt: null);
    }

    /// <summary>
    /// Creates a new <see cref="RefreshToken"/> instance with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the refresh token entity.</param>
    /// <param name="tokenHash">The hashed value of the refresh token.</param>
    /// <param name="userId">The unique identifier of the user associated with this refresh token.</param>
    /// <param name="createdAt">The timestamp when the refresh token was created.</param>
    /// <param name="expiresAt">The timestamp when the refresh token expires.</param>
    /// <param name="revokedAt">The timestamp when the refresh token was revoked, or null if not revoked.</param>
    /// <returns>A new <see cref="RefreshToken"/> instance.</returns>
    public static RefreshToken CreateWithId(
        long id,
        byte[] tokenHash,
        GlobalId userId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt)
    {
        if (createdAt >= expiresAt)
        {
            throw new ArgumentException("createdAt must be earlier than expiresAt.");
        }

        if (revokedAt < createdAt)
        {
            throw new ArgumentException("revokedAt cannot be earlier than createdAt.");
        }

        if (revokedAt > expiresAt)
        {
            throw new ArgumentException("revokedAt cannot be later than expiresAt.");
        }

        return new RefreshToken(
            id,
            tokenHash,
            userId,
            createdAt,
            expiresAt,
            revokedAt);
    }

    /// <summary>
    /// Revokes the refresh token by setting the <see cref="RevokedAt"/> property to the current UTC time.
    /// If the token is already revoked, this method does nothing.
    /// </summary>
    public void Revoke()
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = DateTimeOffset.UtcNow;
    }
}