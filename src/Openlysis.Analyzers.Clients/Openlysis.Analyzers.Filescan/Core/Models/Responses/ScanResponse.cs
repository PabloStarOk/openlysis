using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.Filescan.Core.Models.Responses;

/// <summary>
/// Represents the response from a scan operation.
/// </summary>
/// <param name="FlowId">The unique identifier for the flow.</param>
public record ScanResponse(
    [property: JsonPropertyName("flow_id")] string FlowId);