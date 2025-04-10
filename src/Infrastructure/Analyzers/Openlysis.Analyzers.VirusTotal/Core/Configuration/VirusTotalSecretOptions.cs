using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.VirusTotal.Core.Configuration;

/// <summary>
/// Represents the configuration options for VirusTotal API.
/// </summary>
public record VirusTotalSecretOptions : SecretOptions
{
    /// <summary>
    /// The section name for VirusTotal configuration in the settings file.
    /// </summary>
    public const string SectionName = "VirusTotal";
}