using System.Security.Claims;

using FastEndpoints;

namespace Openlysis.API.Endpoints.Files.Analyze;

/// <summary>
/// Request to analyze a file.
/// </summary>
/// <param name="UserId">The unique identifier of the user making the request, extracted from the authentication claim.</param>
/// <param name="File">File to analyze</param>
/// <param name="Password">Password for the file if it is protected</param>
/// <param name="IsPrivate">Indicates if the file multi analysis should be private.</param>
/// <param name="Reanalyze">If file has already been analyzed by another user, reanalyze it again.</param>
public record AnalyzeFileRequest(
    [property: FromClaim(
        ClaimType = ClaimTypes.NameIdentifier,
        RemoveFromSchema = true)]
    string UserId,
    IFormFile? File,
    string Password = "",
    bool IsPrivate = true,
    bool Reanalyze = false);