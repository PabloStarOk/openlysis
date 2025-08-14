using System.Security.Claims;

using FastEndpoints;

using Microsoft.AspNetCore.Mvc;

namespace Openlysis.API.Endpoints.Common.Requests;

/// <summary>
/// Request to retrieve an analysis by ID.
/// </summary>
/// <param name="UserId">The unique identifier of the user making the request, extracted from the authentication claim.</param>
/// <param name="Id">The ID of the analysis.</param>
public record GetAnalysisByIdRequest(
    [property: FromClaim(
        ClaimType = ClaimTypes.NameIdentifier,
        RemoveFromSchema = true)]
    string UserId,
    [property: FromRoute] string Id);