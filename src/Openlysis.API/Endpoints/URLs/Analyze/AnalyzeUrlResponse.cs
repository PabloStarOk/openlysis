using Openlysis.Domain.URLs;

namespace Openlysis.API.Endpoints.URLs.Analyze;

/// <summary>
/// Represents the response for analyzing a URL.
/// </summary>
/// <param name="Id">The unique identifier for the analysis.</param>
/// <param name="Sha256">The SHA-256 hash of the URL.</param>
/// <param name="Md5">The MD5 hash of the URL.</param>
/// <param name="Sha1">The SHA-1 hash of the URL.</param>
/// <param name="Sha512">The SHA-512 hash of the URL.</param>
public record AnalyzeUrlResponse(
    string Id,
    string Sha256,
    string Md5,
    string Sha1,
    string Sha512)
{
    /// <summary>
    /// Parses a <see cref="UrlMultiAnalysis"/> object into an <see cref="AnalyzeUrlResponse"/> object.
    /// </summary>
    /// <param name="source">The <see cref="UrlMultiAnalysis"/> object to parse.</param>
    /// <returns>An <see cref="AnalyzeUrlResponse"/> object containing the parsed data.</returns>
    public static AnalyzeUrlResponse Parse(UrlMultiAnalysis source)
    {
        return new AnalyzeUrlResponse(
            source.Id.Value.ToString(),
            source.UrlHashSet.Sha256,
            source.UrlHashSet.Md5,
            source.UrlHashSet.Sha1,
            source.UrlHashSet.Sha512);
    }
}