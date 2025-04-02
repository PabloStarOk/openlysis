using Openlysis.Analyzers.Shared.Core.Configuration;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the options for URL query secrets.
/// </summary>
/// <param name="ApiKey">The API key used for authentication.</param>
public record UrlQuerySecretOptions(string ApiKey)
    : SecretOptions(ApiKey)
{
    /// <summary>
    /// The configuration section name for the URL query API key.
    /// </summary>
    public const string SectionName = "UrlQuery";
}