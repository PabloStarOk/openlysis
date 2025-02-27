namespace Openlysis.API.Endpoints.Files.Responses;

/// <summary>
/// Represents a report data transfer object.
/// </summary>
/// <param name="Id">The unique identifier of the report.</param>
/// <param name="Verdict">The verdict of the analysis.</param>
/// <param name="ThreatZone">The threat zone of the analysis.</param>
/// <param name="ThreatLevel">The threat level of the analysis, nullable.</param>
public record ReportDto(
    string Id,
    string Verdict,
    string ThreatZone,
    float? ThreatLevel);