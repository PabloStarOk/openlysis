using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.API.Endpoints.Files.Common.Responses;

/// <summary>
/// Represents the response for a file multi analysis.
/// </summary>
/// <param name="Id">The unique identifier of the file multi analysis.</param>
/// <param name="IsPrivate">Indicates if the analysis is private.</param>
/// <param name="StartedDate">The date and time when the analysis started.</param>
/// <param name="Status">The current status of the file multi analysis.</param>
/// <param name="AverageVerdict">The average verdict of the file multi analysis.</param>
/// <param name="AverageThreatZone">The average threat zone of the file multi analysis.</param>
/// <param name="AverageThreatScore">The average threat score of the file multi analysis, if available.</param>
/// <param name="FileMetadata">Metadata of the file being analyzed.</param>
/// <param name="FileHashSet">A set of content hashes associated with the file.</param>
/// <param name="ServiceAnalyses">The array of service analyses generated from the analysis.</param>
/// <param name="ReportsAmount">The total number of reports generated from the analysis.</param>
public record FileMultiAnalysisDto(
    string Id,
    bool IsPrivate,
    DateTime StartedDate,
    AnalysisStatus Status,
    Verdict AverageVerdict,
    ThreatZone AverageThreatZone,
    float? AverageThreatScore,
    FileMetadata FileMetadata,
    ContentHashSet FileHashSet,
    FileServiceAnalysisDto[] ServiceAnalyses,
    int ReportsAmount)
{
    /// <summary>
    /// Parses a <see cref="FileMultiAnalysis"/> object into a <see cref="FileMultiAnalysisDto"/> object.
    /// </summary>
    /// <param name="source">The source <see cref="FileMultiAnalysis"/> object to parse.</param>
    /// <returns>A <see cref="FileMultiAnalysisDto"/> object.</returns>
    public static FileMultiAnalysisDto Parse(FileMultiAnalysis source)
    {
        FileServiceAnalysisDto[] servicesAnalyses = source.ServiceAnalyses
            .Select(FileServiceAnalysisDto.Parse)
            .ToArray();

        return new FileMultiAnalysisDto(
            source.Id.Value.ToString(),
            source.IsPrivate,
            source.StartedDate,
            source.Status,
            source.AverageVerdict,
            source.AverageThreatZone,
            source.AverageThreatScore,
            source.FileMetadata,
            source.DataHashSet,
            servicesAnalyses,
            source.ReportsAmount);
    }
}