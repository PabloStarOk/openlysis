using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;

namespace Openlysis.Application.Messages.Contracts.Abstractions;

/// <summary>
/// Defines an updater of <see cref="MessageAnalysis"/>.
/// </summary>
public interface IMessageAnalysisUpdater
{
    /// <summary>
    /// Adds a pending <see cref="MessageAnalysis"/> to the system.
    /// </summary>
    /// <param name="messageAnalysis">The message analysis to be added as pending.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddPendingAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the system about the state of a child analysis to update related <see cref="MessageAnalysis"/> objects.
    /// </summary>
    /// <param name="childAnalysisId">The unique identifier of the child analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task NotifyChildAnalysisStateAsync(
        GlobalId childAnalysisId,
        CancellationToken cancellationToken = default);
}