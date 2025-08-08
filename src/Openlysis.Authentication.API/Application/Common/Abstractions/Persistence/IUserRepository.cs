using Openlysis.Domain.Users.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;

/// <summary>
/// Repository for user persistence operations.
/// </summary>
internal interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddAsync(User user);

    /// <summary>
    /// Asynchronously checks if a user exists with the specified email address.
    /// </summary>
    /// <param name="emailAddress">The email address to check for existence.</param>
    /// <returns>True if a user exists with the given email address; otherwise, false.</returns>
    public Task<bool> ExistsAsync(EmailAddress emailAddress);
}