using Openlysis.Domain.FileReports;
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
    /// Gets the analysis of a file.
    /// </summary>
    /// <param name="analysisId">ID of the analysis to get.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="FileAnalysis"/>.</returns>
    public Task<FileAnalysis> GetAnalysisAsync(FileAnalysisId analysisId, CancellationToken cancellationToken);
}