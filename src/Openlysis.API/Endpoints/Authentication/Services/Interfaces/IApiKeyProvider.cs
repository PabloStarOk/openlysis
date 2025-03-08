namespace Openlysis.API.Endpoints.Authentication.Services.Interfaces;

/// <summary>
/// Provides functionality to create API keys.
/// </summary>
public interface IApiKeyProvider
{
    /// <summary>
    /// Creates a new API key.
    /// </summary>
    /// <returns>A string representing the new API key.</returns>
    public string Create();
}