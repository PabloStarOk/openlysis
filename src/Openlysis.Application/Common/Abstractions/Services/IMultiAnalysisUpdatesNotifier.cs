using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;

namespace Openlysis.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a notifier service for updates to a <see cref="MultiAnalysis{TAnalysis}"/>.
/// </summary>
/// <typeparam name="TMultiAnalysis">The type of the multi-analysis aggregate.</typeparam>
/// <typeparam name="TAnalysis">The type of the analysis entity.</typeparam>
public interface IMultiAnalysisUpdatesNotifier<TMultiAnalysis, TAnalysis>
    where TMultiAnalysis : MultiAnalysis<TAnalysis>
    where TAnalysis : Analysis
{
    /// <summary>
    /// Notifies about updates to the specified multi-analysis aggregate.
    /// </summary>
    /// <param name="multiAnalysis">The multi-analysis aggregate to notify about.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task NotifyAsync(TMultiAnalysis multiAnalysis, CancellationToken cancellationToken = default);
}