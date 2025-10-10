using System.Security.Claims;

using FastEndpoints;

namespace Openlysis.API.Endpoints.URLs.Analyze;

/// <summary>
/// Represents a request to analyze a URL.
/// </summary>
/// <param name="UserId">The unique identifier of the user making the request, extracted from the authentication claim.</param>
/// <param name="Url">The URL to be analyzed.</param>
/// <param name="Reanalyze">Indicates whether the URL should be reanalyzed even if an existing analysis is available.</param>
/// <param name="IsPrivate">Indicates whether the URL is private.</param>
public record AnalyzeUrlRequest(
    [property: FromClaim(
        ClaimType = ClaimTypes.NameIdentifier,
        RemoveFromSchema = true)]
    string UserId,
    string Url,
    bool Reanalyze = false,
    bool IsPrivate = false);