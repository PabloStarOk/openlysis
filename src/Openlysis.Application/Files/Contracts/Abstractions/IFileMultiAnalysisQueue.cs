using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.Files.Contracts.Abstractions;

/// <summary>
/// Defines a service to queue files for multi analysis.
/// </summary>
public interface IFileMultiAnalysisQueue
{
    /// <summary>
    /// Queues a file to be analyzed.
    /// </summary>
    /// <param name="multiAnalysisId">The unique identifier for the multi analysis.</param>
    /// <param name="processedFile">The file to be queued for analysis.</param>
    /// <param name="filePassword">The password for the file, if required.</param>
    /// <param name="isPrivateFile">Indicates whether the file is private.</param>
    /// <param name="correlationId">Optional correlation identifier that associates the analysis with a message analysis.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task QueueAsync(
        GlobalId multiAnalysisId,
        ProcessedFile processedFile,
        string filePassword,
        bool isPrivateFile,
        GlobalId? correlationId,
        CancellationToken cancellationToken);
}