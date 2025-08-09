namespace Openlysis.Authentication.API.Endpoints.SignIn;

/// <summary>
/// A request to sign in a new user.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user account.</param>
internal sealed record SignInRequest(
    string Email,
    string Password);