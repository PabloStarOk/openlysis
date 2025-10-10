using System.Security.Claims;

using FastEndpoints;

using Microsoft.AspNetCore.Mvc;

using Openlysis.Application.Common.Enums;

namespace Openlysis.API.Endpoints.Common.Requests;

/// <summary>
/// Request model for getting analyses by hash.
/// </summary>
/// <param name="UserId">The unique identifier of the user making the request, extracted from the authentication claim.</param>
/// <param name="Hash">The hash of the URL to search for analyses.</param>
/// <param name="Page">The page number for pagination. Default is 1.</param>
/// <param name="PageSize">The number of items per page. Default is 10.</param>
/// <param name="StartedDateOrder">The order type for the started date. Default is descending.</param>
public record GetAnalysesByHashRequest(
    [property: FromClaim(
        ClaimType = ClaimTypes.NameIdentifier,
        RemoveFromSchema = true)]
    string UserId,
    [property: FromRoute] string Hash,
    [property: Microsoft.AspNetCore.Mvc.FromQuery] int Page = 1,
    [property: Microsoft.AspNetCore.Mvc.FromQuery] int PageSize = 10,
    [property: Microsoft.AspNetCore.Mvc.FromQuery] OrderType StartedDateOrder = OrderType.Dsc);