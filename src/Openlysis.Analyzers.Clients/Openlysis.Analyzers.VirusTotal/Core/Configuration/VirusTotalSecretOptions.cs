using Openlysis.Analyzers.Contracts.Core.Configuration;

namespace Openlysis.Analyzers.VirusTotal.Core.Configuration;

/// <summary>
/// Represents the configuration options for VirusTotal API.
/// </summary>
/// <param name="ApiKey">The API key for accessing VirusTotal services.</param>
public record VirusTotalSecretOptions(string ApiKey)
    : SecretOptions(ApiKey)
{
    /// <summary>
    /// The section name for VirusTotal configuration in the settings file.
    /// </summary>
    public const string SectionName = "VirusTotal";
}