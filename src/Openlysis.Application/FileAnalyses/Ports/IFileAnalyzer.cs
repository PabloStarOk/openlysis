using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Ports;

/// <summary>
/// Defines an analyzer of files.
/// </summary>
public interface IFileAnalyzer
{
    /// <summary>
    /// Gets the name of the service which provides the analyzer.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Analyzes a file.
    /// </summary>
    /// <param name="fileData">A <see cref="Stream"/> to read the data of the file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ServiceFileAnalysisId"/>.</returns>
    public Task<ServiceFileAnalysisId> AnalyzeAsync(Stream fileData, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an analysis made by the analyzer.
    /// </summary>
    /// <param name="analysisId">A <see cref="ServiceFileAnalysisId"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ServiceFileAnalysis"/>.</returns>
    public Task<ServiceFileAnalysis> GetAnalysisAsync(ServiceFileAnalysisId analysisId, CancellationToken cancellationToken);
}