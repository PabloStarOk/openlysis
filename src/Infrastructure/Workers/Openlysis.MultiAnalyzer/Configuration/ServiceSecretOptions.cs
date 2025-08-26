namespace Openlysis.MultiAnalyzer.Configuration;

/// <summary>
/// Options for configuring service secrets and API keys used by the MultiAnalyzer.
/// </summary>
internal sealed record ServiceSecretOptions
{
    /// <summary>
    /// The configuration section name for API keys.
    /// </summary>
    public const string SectionName = "Secrets";

    /// <summary>
    /// Gets or sets the secret name for the Filescan API key.
    /// </summary>
    public string FilescanApiKeySecretName { get; set; }

    /// <summary>
    /// Gets or sets the secret name for the Hybrid Analysis API key.
    /// </summary>
    public string HybridAnalysisApiKeySecretName { get; set; }

    /// <summary>
    /// Gets or sets the secret name for the URL Query API key.
    /// </summary>
    public string UrlQueryApiKeySecretName { get; set; }

    /// <summary>
    /// Gets or sets the secret name for the VirusTotal API key.
    /// </summary>
    public string VirusTotalApiKeySecretName { get; set; }

    /// <summary>
    /// Gets or sets the secret name for the Google Cloud Storage (GCS) credentials.
    /// </summary>
    public string GcsCredentialSecretName { get; set; }
}