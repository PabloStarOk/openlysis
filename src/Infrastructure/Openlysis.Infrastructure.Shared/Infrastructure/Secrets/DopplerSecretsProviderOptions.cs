namespace Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

/// <summary>
/// Options for configuring Doppler secrets provider.
/// </summary>
public sealed record DopplerSecretsProviderOptions
{
    /// <summary>
    /// Gets or sets the names of the secrets containing Doppler API keys.
    /// </summary>
    required public string[] ApiKeySecretNames { get; set; }

    required public string GcsCredentialSecretName { get; set; }
}