using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Enums;

namespace Openlysis.API.Endpoints.Analyses.File.Responses;

/// <summary>
/// Represents the response which contains a <see cref="FileAnalysis"/>
/// </summary>
/// <param name="Id">The unique identifier of the file analysis.</param>
/// <param name="LastScanDate">The date and time of the last scan.</param>
/// <param name="ReportsAmount">The number of reports generated.</param>
/// <param name="Verdict">The verdict of the file analysis.</param>
/// <param name="File">The file being analyzed.</param>
/// <param name="Reports">The array of reports generated from the analysis.</param>
public record FileAnalysisResponse(
    string Id,
    DateTime LastScanDate,
    int ReportsAmount,
    Verdict Verdict,
    Domain.FileAnalyses.ValueObjects.File File,
    Report[] Reports);