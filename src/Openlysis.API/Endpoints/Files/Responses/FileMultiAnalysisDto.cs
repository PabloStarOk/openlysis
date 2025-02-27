using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.API.Endpoints.Files.Responses;

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
    int ReportsAmount)
{
    /// <summary>
    /// Parses a <see cref="FileMultiAnalysis"/> object into a <see cref="FileMultiAnalysisDto"/> object.
    /// </summary>
    /// <param name="source">The source <see cref="FileMultiAnalysis"/> object to parse.</param>
    /// <returns>A <see cref="FileMultiAnalysisDto"/> object.</returns>
    public static FileMultiAnalysisDto Parse(FileMultiAnalysis source)
    {
        // Parse the service analyses from the source object
        var serviceAnalyses = source.ServiceFileAnalyses.Select(
            s =>
            {
                // Parse the reports from the service analysis
                IEnumerable<ReportDto> reportDtos = s.Reports
                    .Select(
                        r => new ReportDto(
                            r.Id.Value,
                            r.Verdict.ToString(),
                            r.ThreatZone.ToString(),
                            r.ThreatLevel));

                // Return a new ServiceFileAnalysisDto object
                return new ServiceFileAnalysisDto(
                    s.ServiceName,
                    s.Status.ToString(),
                    reportDtos);
            });

        // Return a new FileMultiAnalysisDto object
        return new FileMultiAnalysisDto(
            source.Id.Value.ToString(),
            source.StartedDate,
            source.AverageVerdict.ToString(),
            source.AverageThreatZone.ToString(),
            source.Status.ToString(),
            source.FileMetadata,
            source.ContentHashSet,
            serviceAnalyses,
            source.ReportsAmount);
    }
}