using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a database user entity for persistence.
/// </summary>
internal sealed record DbUser
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    required public string EmailAddress { get; init; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    required public byte[] PasswordHash { get; init; }

    /// <summary>
    /// Gets the salt used for hashing the user's password.
    /// </summary>
    required public byte[] PasswordHashSalt { get; init; }

    /// <summary>
    /// Converts this database user entity to a domain user entity.
    /// </summary>
    /// <returns>A <see cref="User"/> domain entity.</returns>
    public User ToDomainUser()
    {
        var id = GlobalId.Parse(UserId.ToString());
        var emailAddress = new EmailAddress(EmailAddress);
        return User.CreateWithId(
            id,
            emailAddress,
            PasswordHash,
            PasswordHashSalt);
    }
}