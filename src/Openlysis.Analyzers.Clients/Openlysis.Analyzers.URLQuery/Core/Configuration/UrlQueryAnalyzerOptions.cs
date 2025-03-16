using Openlysis.Analyzers.Contracts.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the options for configuring the URL query client.
/// </summary>
public record UrlQueryAnalyzerOptions : AnalyzerOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "UrlQuery:Analyzer";

    /// <summary>
    /// Gets the default user agent type.
    /// </summary>
    required public UserAgentType DefaultUserAgent { get; init; }
}