using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.Files.Contracts.Abstractions;

/// <summary>
/// Defines a service to analyze a file.
/// </summary>
public interface IFileMultiAnalyzer
{
    /// <summary>
    /// Starts the analysis process for a given file as part of a multi analysis.
    /// </summary>
    /// <param name="multiAnalysisId">The unique identifier for the multi-file analysis session.</param>
    /// <param name="processedFile">The file to be analyzed.</param>
    /// <param name="filePassword">The password for the file, if required.</param>
    /// <param name="isPrivateFile">Indicates whether the file is private.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task StartAnalysisAsync(
        GlobalId multiAnalysisId,
        ProcessedFile processedFile,
        string filePassword,
        bool isPrivateFile,
        CancellationToken cancellationToken);
}