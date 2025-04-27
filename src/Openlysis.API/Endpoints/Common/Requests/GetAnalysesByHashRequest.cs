using Microsoft.AspNetCore.Mvc;

using Openlysis.Application.Common.Enums;

namespace Openlysis.API.Endpoints.Common.Requests;

/// <summary>
/// Request model for getting analyses by hash.
/// </summary>
/// <param name="Hash">The hash of the URL to search for analyses.</param>
/// <param name="Amount">The number of analyses to return. Default is 10.</param>
/// <param name="StartedDateOrder">The order type for the date. Default is descending.</param>
public record GetAnalysesByHashRequest(
    [property: FromRoute] string Hash,
    [property: FromQuery] int Amount = 10,
    [property: FromQuery] OrderType StartedDateOrder = OrderType.Dsc);