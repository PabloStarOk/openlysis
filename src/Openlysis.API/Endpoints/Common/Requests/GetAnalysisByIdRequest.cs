using Microsoft.AspNetCore.Mvc;

namespace Openlysis.API.Endpoints.Common.Requests;

/// <summary>
/// Request to retrieve an analysis by ID.
/// </summary>
/// <param name="Id">The ID of the analysis.</param>
public record GetAnalysisByIdRequest([property: FromRoute] string Id);