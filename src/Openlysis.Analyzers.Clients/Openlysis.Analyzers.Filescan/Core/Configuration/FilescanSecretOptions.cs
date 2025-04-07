using Openlysis.Infrastructure.Shared.Configuration;

namespace Openlysis.Analyzers.Filescan.Core.Configuration;

/// <summary>
/// Represents the options for the Filescan secret configuration.
/// </summary>
/// <param name="ApiKey">The API key used for authentication.</param>
public record FilescanSecretOptions(string ApiKey)
    : SecretOptions(ApiKey)
{
    /// <summary>
    /// The section name for the Filescan configuration.
    /// </summary>
    public const string SectionName = "Filescan";
}