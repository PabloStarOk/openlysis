using Microsoft.AspNetCore.Mvc;

namespace Openlysis.API.Endpoints.Sms.GetAnalysisById;

/// <summary>
/// Represents a request to get an analysis by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the analysis, provided via the route.</param>
public record GetAnalysisByIdRequest([FromRoute] Guid Id);