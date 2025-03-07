namespace Openlysis.API.Endpoints.Authentication.SignUp;

/// <summary>
/// Represents a request to register a new user.
/// </summary>
/// <param name="UserName">The username of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user account.</param>
public record SignUpRequest(string UserName, string Email, string Password);