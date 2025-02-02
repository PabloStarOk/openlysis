using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileReports.Entities;
using Openlysis.Domain.FileReports.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Ports;

/// <summary>
/// Defines an analyzer of files.
/// </summary>
public interface IFileAnalyzer
{
    /// <summary>
    /// Analyzes a file.
    /// </summary>
    /// <param name="fileData">Data of file to be analyzed.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="FileAnalysisId"/>.</returns>
    public Task<FileAnalysisId> AnalyzeAsync(byte[] fileData, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all reports for a file analysis.
    /// </summary>
    /// <param name="analysisId">A <see cref="analysisId"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An array of <see cref="Report"/>.</returns>
    public Task<FileReport[]> GetReportsAsync(FileAnalysisId analysisId, CancellationToken cancellationToken);
}