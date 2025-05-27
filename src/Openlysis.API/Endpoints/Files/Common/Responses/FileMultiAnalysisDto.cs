using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.API.Endpoints.Files.Common.Responses;

/// <summary>
/// Data transfer object for <see cref="FileMultiAnalysis"/>.
/// </summary>
/// <param name="Id">The unique identifier of the file multi analysis.</param>
/// <param name="IsPrivate">Indicates if the analysis is private.</param>
/// <param name="StartedDate">The date and time when the analysis started.</param>
/// <param name="Status">The current status of the file multi analysis.</param>
/// <param name="FinalVerdict">The final verdict of the file multi analysis.</param>
/// <param name="FinalThreatZone">The final threat zone of the file multi analysis.</param>
/// <param name="AverageThreatScore">The average threat score of the file multi analysis, if available.</param>
/// <param name="FileMetadata">Metadata of the file being analyzed.</param>
/// <param name="FileHashValues">A set of content hashes associated with the file.</param>
/// <param name="ServiceAnalyses">The array of service analyses generated from the analysis.</param>
/// <param name="ReportsAmount">The total number of reports generated from the analysis.</param>
public record FileMultiAnalysisDto(
    string Id,
    bool IsPrivate,
    DateTime StartedDate,
    AnalysisStatus Status,
    Verdict FinalVerdict,
    ThreatZone FinalThreatZone,
    float? AverageThreatScore,
    FileMetadata FileMetadata,
    HashValues FileHashValues,
    FileAnalysisDto[] ServiceAnalyses,
    int ReportsAmount)
{
    /// <summary>
    /// Parses a <see cref="FileMultiAnalysis"/> object into a <see cref="FileMultiAnalysisDto"/> object.
    /// </summary>
    /// <param name="source">The source <see cref="FileMultiAnalysis"/> object to parse.</param>
    /// <returns>A <see cref="FileMultiAnalysisDto"/> object.</returns>
    public static FileMultiAnalysisDto Parse(FileMultiAnalysis source)
    {
        FileAnalysisDto[] servicesAnalyses = source.Analyses
            .Select(FileAnalysisDto.Parse)
            .ToArray();

        return new FileMultiAnalysisDto(
            source.Id.Value.ToString(),
            source.IsPrivate,
            source.StartedDate,
            source.State.Status,
            source.State.Verdict,
            source.State.ThreatZone,
            source.AverageThreatScore,
            source.FileMetadata,
            source.DataHashValues,
            servicesAnalyses,
            source.ReportsAmount);
    }
}