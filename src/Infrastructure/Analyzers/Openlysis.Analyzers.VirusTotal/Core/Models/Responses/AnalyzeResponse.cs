using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.VirusTotal.Core.Models.Responses;

/// <summary>
/// Represents the response received after submitting a file or URL for analysis.
/// </summary>
/// <param name="AnalysisId">The unique identifier of the analysis, mapped from the "id" JSON property.</param>
public record AnalyzeResponse(
    [property: JsonPropertyName("id")] string AnalysisId);