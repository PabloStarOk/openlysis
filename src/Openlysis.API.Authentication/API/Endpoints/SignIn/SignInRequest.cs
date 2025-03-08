namespace Openlysis.API.Authentication.API.Endpoints.SignIn;

/// <summary>
/// Represents a request to sign in with an email and password.
/// </summary>
/// <param name="UserName">The name of the user.</param>
/// <param name="Password">The password of the user.</param>
/// <param name="TwoFactorCode">The optional two-factor authentication code.</param>
/// <param name="TwoFactorRecoveryCode">The optional two-factor recovery code.</param>
public record SignInRequest(
    string UserName,
    string Password,
    string? TwoFactorCode = null,
    string? TwoFactorRecoveryCode = null);