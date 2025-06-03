using MassTransit;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.Common;

/// <summary>
/// MassTransit consumer that handles <see cref="UpdateMultiAnalysis{TAnalysis}"/> messages,
/// updating or adding analyses to a <typeparamref name="TMultiAnalysis"/> aggregate.
/// </summary>
/// <typeparam name="TMultiAnalysis">The multi-analysis aggregate type.</typeparam>
/// <typeparam name="TAnalysis">The analysis entity type.</typeparam>
internal sealed class UpdateMultiAnalysisConsumer<TMultiAnalysis, TAnalysis>
    : IConsumer<UpdateMultiAnalysis<TAnalysis>>
    where TMultiAnalysis : MultiAnalysis<TAnalysis>
    where TAnalysis : Analysis
{
    private readonly IRepository<TMultiAnalysis, GlobalId> _repository;
    private readonly IMessageAnalysisUpdater _messageAnalysisUpdater;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMultiAnalysisConsumer{TMultiAnalysis, TAnalysis}"/> class.
    /// </summary>
    /// <param name="repository">The repository for accessing and updating the multi-analysis aggregate.</param>
    /// <param name="messageAnalysisUpdater">The service for notifying about child analysis state changes.</param>
    public UpdateMultiAnalysisConsumer(
        IRepository<TMultiAnalysis, GlobalId> repository,
        IMessageAnalysisUpdater messageAnalysisUpdater)
    {
        _repository = repository;
        _messageAnalysisUpdater = messageAnalysisUpdater;
    }

    /// <inheritdoc/>
    public async Task Consume(
        ConsumeContext<UpdateMultiAnalysis<TAnalysis>> context)
    {
        UpdateMultiAnalysis<TAnalysis> message = context.Message;
        TMultiAnalysis? multiAnalysis = await _repository.GetAsync(
            message.MultiAnalysisId,
            context.CancellationToken);
        ArgumentNullException.ThrowIfNull(multiAnalysis);

        foreach (var analysis in message.UpdatableAnalyses)
        {
            if (multiAnalysis.Analyses.Contains(analysis))
            {
                multiAnalysis.UpdateAnalysis(analysis);
                continue;
            }

            multiAnalysis.AddAnalysis(analysis);
        }

        if (message.Timeout)
        {
            multiAnalysis.SetAsTimedOut();
        }

        await _repository.UpdateAsync(multiAnalysis, context.CancellationToken);

        await _messageAnalysisUpdater.NotifyChildAnalysisStateAsync(
            multiAnalysis.Id,
            context.CancellationToken);
    }
}