using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Enums;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.API.Endpoints.Analyses.File.Responses;

/// <summary>
/// Represents the response which contains a <see cref="FileAnalysis"/>
/// </summary>
/// <param name="Id">The unique identifier of the file analysis.</param>
/// <param name="LastScanDate">The date and time of the last scan.</param>
/// <param name="ReportsAmount">The number of reports generated.</param>
/// <param name="Verdict">The verdict of the file analysis.</param>
/// <param name="Metadata">Metadata of the file.</param>
/// <param name="Reports">The array of reports generated from the analysis.</param>
public record FileAnalysisResponse(
    string Id,
    DateTime LastScanDate,
    int ReportsAmount,
    Verdict Verdict,
    FileMetadata Metadata,
    Report[] Reports);