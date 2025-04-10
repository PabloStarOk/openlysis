using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the options for URL query secrets.
/// </summary>
public record UrlQuerySecretOptions : SecretOptions
{
    /// <summary>
    /// The configuration section name for the URL query API key.
    /// </summary>
    public const string SectionName = "UrlQuery";
}