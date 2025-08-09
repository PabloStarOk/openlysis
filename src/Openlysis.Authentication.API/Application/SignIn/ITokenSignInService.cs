using ErrorOr;

using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Application.SignIn;

/// <summary>
/// Defines a service to provide sign in users based on authentication tokens.
/// </summary>
internal interface ITokenSignInService
{
    /// <summary>
    /// Attempts to sign in a user with the provided email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an <see cref="ErrorOr{T}"/> with the authentication token as a string if successful,
    /// or an error if sign-in fails.
    /// </returns>
    public Task<ErrorOr<string>> SignInAsync(EmailAddress email, string password);
}