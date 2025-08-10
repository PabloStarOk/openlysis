using ErrorOr;

namespace Openlysis.Authentication.API.Application.Common.Models;

/// <summary>
/// Contains authentication-related error definitions.
/// </summary>
internal static class AuthError
{
    /// <summary>
    /// Gets the error indicating that a user with the provided email address already exists.
    /// </summary>
    public static Error EmailAlreadyExists { get; } = Error.Custom(
        (int)ErrorType.Conflict,
        "User.EmailAlreadyExists",
        "An user with the provided email address already exists.");
}