using System.Text.Json.Serialization;

using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Analyzers.Filescan.Core.Models.Objects;

namespace Openlysis.Analyzers.Filescan.Core.Models.Responses;

/// <summary>
/// Represents the response for an analysis request.
/// </summary>
/// <param name="FlowId">The unique identifier for the flow.</param>
/// <param name="Status">The current status of the analysis.</param>
/// <param name="Reports">A dictionary containing the analysis reports.</param>
public record GetAnalysisResponse(
    [property: JsonPropertyName("flowId")] string FlowId,
    [property: JsonPropertyName("state")] Status Status,
    [property: JsonPropertyName("reports")] Dictionary<string, FilescanReport> Reports);