namespace Openlysis.API.Endpoints.URLs.Analyze;

/// <summary>
/// Represents a request to analyze a URL.
/// </summary>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="Reanalyze">Indicates whether the URL should be reanalyzed even if an existing analysis is available.</param>
/// <param name="IsPrivate">Indicates whether the URL is private.</param>
public record AnalyzeUrlRequest(
    string Url,
    bool Reanalyze = false,
    bool IsPrivate = false);