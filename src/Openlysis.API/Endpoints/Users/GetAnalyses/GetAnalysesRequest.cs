using System.Security.Claims;

using FastEndpoints;

namespace Openlysis.API.Endpoints.Users.GetAnalyses;

/// <summary>
/// Request for retrieving analyses started by the user.
/// </summary>
/// <param name="UserId">The unique identifier of the user making the request, extracted from the authentication claim.</param>
/// <param name="Type">The type of analysis to filter by (Url, File, or Message).</param>
/// <param name="Page">The page number for pagination (default is 1).</param>
/// <param name="PageSize">The number of items per page (default is 10).</param>
public record GetAnalysesRequest(
    [property: FromClaim(
        ClaimType = ClaimTypes.NameIdentifier,
        RemoveFromSchema = true)]
    string UserId,
    [property: QueryParam] AnalysisType Type,
    [property: QueryParam] int Page = 1,
    [property: QueryParam] int PageSize = 10);