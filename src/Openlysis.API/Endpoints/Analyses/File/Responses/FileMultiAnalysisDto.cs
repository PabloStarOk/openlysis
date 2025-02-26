using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.API.Endpoints.Analyses.File.Responses;

/// <summary>
/// Represents the response for a file multi analysis.
/// </summary>
/// <param name="Id">The unique identifier of the file multi analysis.</param>
/// <param name="StartedDate">The date and time when the analysis started.</param>
/// <param name="AverageVerdict">The average verdict of the file multi analysis.</param>
/// <param name="AverageThreatZone">The average threat zone of the file multi analysis.</param>
/// <param name="Status">The status of the file multi analysis.</param>
/// <param name="FileMetadata">Metadata of the file.</param>
/// <param name="HashSet">The set of content hashes associated with the file.</param>
/// <param name="ServiceAnalyses">The array of service analyses generated from the analysis.</param>
/// <param name="ReportsAmount">The number of reports generated.</param>
public record FileMultiAnalysisDto(
    string Id,
    DateTime StartedDate,
    string AverageVerdict,
    string AverageThreatZone,
    string Status,
    FileMetadata FileMetadata,
    ContentHashSet HashSet,
    IEnumerable<ServiceFileAnalysisDto> ServiceAnalyses,
    int ReportsAmount);