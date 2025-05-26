using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files.Entities;

namespace Openlysis.API.Endpoints.Files.Common.Responses;

/// <summary>
/// Represents a report data transfer object.
/// </summary>
/// <param name="Id">The unique identifier of the report.</param>
/// <param name="Verdict">The verdict of the analysis.</param>
/// <param name="ThreatZone">The threat zone of the analysis.</param>
/// <param name="ThreatScore">The threat score of the analysis, if available.</param>
public record FileReportDto(
    string Id,
    Verdict Verdict,
    ThreatZone ThreatZone,
    int? ThreatScore)
{
    /// <summary>
    /// Converts a <see cref="FileReport"/> entity to a <see cref="FileReportDto"/>.
    /// </summary>
    /// <param name="fileReport">The <see cref="FileReport"/> entity to convert.</param>
    /// <returns>A new instance of <see cref="FileReportDto"/> populated with data from the given <see cref="FileReport"/>.</returns>
    public static FileReportDto Parse(FileReport fileReport)
    {
        return new FileReportDto(
            fileReport.Id.Value,
            fileReport.Verdict,
            fileReport.ThreatZone,
            fileReport.ThreatScore.NormalizedValue);
    }
}