namespace Openlysis.API.Endpoints.Authentication.ApiKeyReset;

/// <summary>
/// Represents a request to reset an API key.
/// </summary>
/// <param name="UserName">The username of the account.</param>
/// <param name="Password">The password of the account.</param>
/// <param name="TwoFactorCode">The two-factor authentication code.</param>
/// <param name="TwoFactorRecoveryCode">The two-factor authentication recovery code.</param>
public record ApiKeyResetRequest(
    string UserName,
    string Password,
    string? TwoFactorCode = null,
    string? TwoFactorRecoveryCode = null);