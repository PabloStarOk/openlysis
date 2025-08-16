namespace Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

/// <summary>
/// Provides a method to retrieve API keys by their secret name.
/// </summary>
public interface IApiKeyProvider
{
    /// <summary>
    /// Retrieves the API key associated with the specified secret name.
    /// </summary>
    /// <param name="apiKeySecretName">The name of the API key secret.</param>
    /// <returns>The API key as a string.</returns>
    string GetApiKey(string apiKeySecretName);
}