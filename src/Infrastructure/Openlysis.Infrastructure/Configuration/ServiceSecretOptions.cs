namespace Openlysis.Infrastructure.Configuration;

/// <summary>
/// Options for configuring service secrets, such as API key secret names.
/// </summary>
internal sealed record ServiceSecretOptions
{
    /// <summary>
    /// The configuration section name for API keys.
    /// </summary>
    public const string SectionName = "ApiKeys";

    /// <summary>
    /// Gets or sets the secret name used to retrieve the IPQS API key.
    /// </summary>
    required public string IpqsApiKeySecretName { get; set; }
}