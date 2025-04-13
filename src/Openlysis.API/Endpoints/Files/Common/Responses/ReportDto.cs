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
public record ReportDto(
    string Id,
    Verdict Verdict,
    ThreatZone ThreatZone,
    float? ThreatScore)
{
    /// <summary>
    /// Converts a <see cref="Report"/> entity to a <see cref="ReportDto"/>.
    /// </summary>
    /// <param name="report">The <see cref="Report"/> entity to convert.</param>
    /// <returns>A new instance of <see cref="ReportDto"/> populated with data from the given <see cref="Report"/>.</returns>
    public static ReportDto Parse(Report report)
    {
        return new ReportDto(
            report.Id.Value,
            report.Verdict,
            report.ThreatZone,
            report.ThreatScore);
    }
}