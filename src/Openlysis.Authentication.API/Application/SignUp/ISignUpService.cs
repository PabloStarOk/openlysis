using ErrorOr;

using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Application.SignUp;

/// <summary>
/// Service interface for handling user sign-up operations.
/// </summary>
internal interface ISignUpService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>
    /// A <see cref="Success"/> result or an error.
    /// </returns>
    public Task<ErrorOr<Success>> SignUpUserAsync(EmailAddress email, string password);
}