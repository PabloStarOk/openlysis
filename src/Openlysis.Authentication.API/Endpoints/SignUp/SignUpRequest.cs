namespace Openlysis.Authentication.API.Endpoints.SignUp;

/// <summary>
/// A request to sign up a new user.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user account.</param>
internal sealed record SignUpRequest(
    string Email,
    string Password);