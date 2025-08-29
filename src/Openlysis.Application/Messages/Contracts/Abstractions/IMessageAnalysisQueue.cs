using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;

namespace Openlysis.Application.Messages.Contracts.Abstractions;

/// <summary>
/// Represents a queue for message analysis operations.
/// </summary>
public interface IMessageAnalysisQueue
{
    /// <summary>
    /// Enqueues a message analysis operation for processing.
    /// </summary>
    /// <param name="correlationId">The correlation identifier for the operation.</param>
    /// <param name="analysis">The message analysis to enqueue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task EnqueueAsync(
        GlobalId correlationId,
        MessageAnalysis analysis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the analysis queue as initialized for the given correlation identifier.
    /// </summary>
    /// <param name="correlationId">The correlation identifier to initialize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SetAsInitializedAsync(
        GlobalId correlationId,
        CancellationToken cancellationToken = default);
}