using System.Text.Json.Serialization;

using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

namespace Openlysis.Analyzers.VirusTotal.Core.Models.Responses;

/// <summary>
/// Represents the response from the VirusTotal analysis API.
/// </summary>
/// <param name="Id">The unique identifier of the analysis.</param>
/// <param name="Attributes">The attributes of the analysis.</param>
public record GetAnalysisResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("attributes")] Attributes Attributes);