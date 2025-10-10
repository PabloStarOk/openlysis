using System.Text.Json.Serialization;

using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Responses;

/// <summary>
/// Represents the response for getting a report overview.
/// </summary>
/// <param name="ReportId">The unique identifier of the report.</param>
/// <param name="Status">The status of the report.</param>
internal record GetReportOverviewResponse(
    [property: JsonPropertyName("report_id")] string ReportId,
    [property: JsonPropertyName("status")] Status Status);