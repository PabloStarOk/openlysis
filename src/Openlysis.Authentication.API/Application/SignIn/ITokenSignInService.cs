using ErrorOr;

using Openlysis.Authentication.API.Application.Common.Models;
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
    /// A task that represents the asynchronous operation. The task result contains either authentication tokens or an error.
    /// </returns>
    public Task<ErrorOr<AuthTokens>> SignInAsync(
        EmailAddress email,
        string password);
}