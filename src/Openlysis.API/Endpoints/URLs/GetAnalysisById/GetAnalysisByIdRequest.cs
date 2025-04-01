using Microsoft.AspNetCore.Mvc;

using Openlysis.Domain.URLs;

namespace Openlysis.API.Endpoints.URLs.GetAnalysisById;

/// <summary>
/// Request to retrieve a <see cref="UrlMultiAnalysis"/> by ID.
/// </summary>
/// <param name="Id">The ID of the analysis.</param>
public record GetAnalysisByIdRequest(
    [property: FromRoute] Guid Id);