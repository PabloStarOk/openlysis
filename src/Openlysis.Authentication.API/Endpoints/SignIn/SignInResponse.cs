namespace Openlysis.Authentication.API.Endpoints.SignIn;

/// <summary>
/// Represents the response returned after a successful sign-in, containing the access token.
/// </summary>
internal sealed record SignInResponse(string AccessToken);