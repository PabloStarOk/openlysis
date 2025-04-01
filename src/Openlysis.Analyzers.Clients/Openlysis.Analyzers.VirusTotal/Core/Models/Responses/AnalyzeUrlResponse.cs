using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.VirusTotal.Core.Models.Responses;

/// <summary>
/// Represents the response from analyzing a URL of VirusTotal.
/// </summary>
/// <param name="AnalysisId">The ID of the analysis.</param>
public record AnalyzeUrlResponse(
    [property: JsonPropertyName("id")] string AnalysisId);