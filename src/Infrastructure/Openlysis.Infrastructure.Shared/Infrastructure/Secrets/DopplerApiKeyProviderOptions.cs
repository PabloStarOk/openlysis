namespace Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

/// <summary>
/// Options for providing Doppler API key secrets.
/// </summary>
public sealed record DopplerApiKeyProviderOptions
{
    /// <summary>
    /// Gets or sets the names of the secrets containing Doppler API keys.
    /// </summary>
    required public string[] ApiKeySecretNames { get; set; }
}