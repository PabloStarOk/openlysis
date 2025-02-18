using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.API.Endpoints.Analyses.File.Responses;

/// <summary>
/// Represents the response which contains a <see cref="FileMultiAnalysis"/>
/// </summary>
/// <param name="Id">The unique identifier of the file analysis.</param>
/// <param name="StartedDate">The date and time when the analysis started.</param>
/// <param name="Verdict">The verdict of the file analysis.</param>
/// <param name="Metadata">Metadata of the file.</param>
/// <param name="Reports">The array of reports generated from the analysis.</param>
/// <param name="ReportsAmount">The number of reports generated.</param>
public record FileAnalysisResponse(
    string Id,
    DateTime StartedDate,
    Verdict Verdict,
    FileMetadata Metadata,
    IEnumerable<Report> Reports,
    int ReportsAmount);