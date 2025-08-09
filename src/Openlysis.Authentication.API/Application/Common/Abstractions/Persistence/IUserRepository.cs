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
    /// Retrieves a user entity by the specified email address.
    /// </summary>
    /// <param name="emailAddress">The email address of the user to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{User}"/> representing the asynchronous operation,
    /// containing the user entity if found; otherwise, null.
    /// </returns>
    public Task<User> GetByEmailAsync(EmailAddress emailAddress);

    /// <summary>
    /// Asynchronously checks if a user exists with the specified email address.
    /// </summary>
    /// <param name="emailAddress">The email address to check for existence.</param>
    /// <returns>True if a user exists with the given email address; otherwise, false.</returns>
    public Task<bool> ExistsAsync(EmailAddress emailAddress);
}