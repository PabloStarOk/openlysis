using Openlysis.Domain.Files;

namespace Openlysis.Application.Files.Contracts.Abstractions;

/// <summary>
/// Defines a service to analyze a file.
/// </summary>
public interface IFileMultiAnalyzer
{
    /// <summary>
    /// Starts a multi analysis job asynchronously.
    /// </summary>
    /// <param name="fileMultiAnalysis">The file multi-analysis details.</param>
    /// <param name="fileData">The stream containing the file data.</param>
    /// <param name="filePassword">The password for the file, if any.</param>
    /// <param name="isPrivateFile">Indicates whether the file is private.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task StartAnalysisAsync(
        FileMultiAnalysis fileMultiAnalysis,
        Stream fileData,
        string filePassword,
        bool isPrivateFile,
        CancellationToken cancellationToken);
}