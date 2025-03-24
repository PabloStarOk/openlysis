using System.Text.Json.Serialization;

using Openlysis.Analyzers.URLQuery.Core.Models.Objects;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Responses;

/// <summary>
/// Represents the response for getting a report.
/// </summary>
/// <param name="ReportId">The unique identifier of the report.</param>
/// <param name="Sensors">The sensors associated with the report.</param>
public record GetReportResponse(
    [property: JsonPropertyName("report_id")] string ReportId,
    [property: JsonPropertyName("sensors")] Sensors Sensors);