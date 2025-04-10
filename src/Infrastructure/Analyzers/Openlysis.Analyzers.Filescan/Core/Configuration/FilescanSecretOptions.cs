using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.Filescan.Core.Configuration;

/// <summary>
/// Represents the options for the Filescan secret configuration.
/// </summary>
public record FilescanSecretOptions : SecretOptions
{
    /// <summary>
    /// The section name for the Filescan configuration.
    /// </summary>
    public const string SectionName = "Filescan";
}