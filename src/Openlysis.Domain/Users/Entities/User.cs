using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Domain.Users.Entities;

/// <summary>
/// Represents a user entity with an email and password hash.
/// </summary>
public sealed class User : Entity<GlobalId>
{
    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public EmailAddress Email { get; }

    /// <summary>
    /// Gets the user's password hash as a byte array.
    /// </summary>
    public byte[] PasswordHash { get; }

    /// <summary>
    /// Gets the salt used for hashing the user's password.
    /// </summary>
    public byte[] PasswordHashSalt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class with the specified ID, email, password hash, and password hash salt.
    /// </summary>
    /// <param name="id">The unique identifier for the user.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The user's password hash as a byte array.</param>
    /// <param name="passwordHashSalt">The salt used for hashing the user's password.</param>
    private User(
        GlobalId id,
        EmailAddress email,
        byte[] passwordHash,
        byte[] passwordHashSalt)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        PasswordHashSalt = passwordHashSalt;
    }

    /// <summary>
    /// Creates a new <see cref="User"/> instance with the specified email and password hash.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The user's password hash as a byte array.</param>
    /// <param name="passwordHashSalt">The salt used for hashing the user's password.</param>
    /// <returns>A new <see cref="User"/> object.</returns>
    public static User Create(
        EmailAddress email,
        byte[] passwordHash,
        byte[] passwordHashSalt)
    {
        var id = GlobalId.CreateUnique();
        return new User(id, email, passwordHash, passwordHashSalt);
    }

    /// <summary>
    /// Creates a new <see cref="User"/> instance with the specified ID, email, password hash, and password hash salt.
    /// </summary>
    /// <param name="id">The unique identifier for the user.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The user's password hash as a byte array.</param>
    /// <param name="passwordHashSalt">The salt used for hashing the user's password.</param>
    /// <returns>A new <see cref="User"/> object.</returns>
    public static User CreateWithId(
        GlobalId id,
        EmailAddress email,
        byte[] passwordHash,
        byte[] passwordHashSalt)
    {
        return new User(id, email, passwordHash, passwordHashSalt);
    }
}