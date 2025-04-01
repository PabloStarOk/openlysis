namespace Openlysis.API.Endpoints.URLs.Analyze;

/// <summary>
/// Represents a request to analyze a URL.
/// </summary>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="IsPrivate">Indicates whether the URL is private.</param>
public record AnalyzeUrlRequest(
    string Url,
    bool IsPrivate = false);