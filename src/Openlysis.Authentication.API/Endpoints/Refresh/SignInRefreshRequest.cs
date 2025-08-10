namespace Openlysis.Authentication.API.Endpoints.Refresh;

/// <summary>
/// Request model for refreshing a sign-in session using a refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token used to obtain a new access token.</param>
internal sealed record SignInRefreshRequest(string RefreshToken);