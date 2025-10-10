using Openlysis.Domain.Messages;

namespace Openlysis.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a notifier service for updates to a <see cref="MessageAnalysis"/>.
/// </summary>
public interface IMessageAnalysisUpdatesNotifier
{
    /// <summary>
    /// Notifies about updates to the specified message analysis aggregate.
    /// </summary>
    /// <param name="analysis">The message analysis aggregate to notify about.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task NotifyAsync(MessageAnalysis analysis, CancellationToken cancellationToken = default);
}