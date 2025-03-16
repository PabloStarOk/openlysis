using System.Text.Json.Serialization;

using Openlysis.Analyzers.URLQuery.Core.Models.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Responses;

/// <summary>
/// Represents the response received after submitting a URL.
/// </summary>
/// <param name="QueueId">The ID of the queue.</param>
/// <param name="ReportId">The ID of the report.</param>
/// <param name="Status">The status of the submission.</param>
internal record SubmitUrlResponse(
    [property: JsonPropertyName("queue_id")] string QueueId,
    [property: JsonPropertyName("report_id")] string ReportId,
    [property: JsonPropertyName("status")] Status Status);