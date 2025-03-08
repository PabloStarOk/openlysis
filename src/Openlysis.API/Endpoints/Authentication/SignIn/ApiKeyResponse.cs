namespace Openlysis.API.Endpoints.Authentication.SignIn;

/// <summary>
/// Represents a response that contains an API Key.
/// </summary>
/// <param name="ApiKey">A <see cref="string"/> representing the API Key.</param>
public record ApiKeyResponse(string ApiKey);