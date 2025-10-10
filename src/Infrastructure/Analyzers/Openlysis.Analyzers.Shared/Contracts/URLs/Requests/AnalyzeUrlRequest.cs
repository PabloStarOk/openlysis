using Openlysis.Analyzers.Shared.Contracts.Common.Requests;

namespace Openlysis.Analyzers.Shared.Contracts.URLs.Requests;

/// <summary>
/// Represents a request to analyze a URL.
/// </summary>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="Description">A description of the URL.</param>
/// <param name="IsPrivate">Indicates whether the URL is private.</param>
public record AnalyzeUrlRequest(
    Uri Url,
    string Description = "",
    bool IsPrivate = false)
    : AnalyzeRequest(Description, IsPrivate);