using MassTransit;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.URLs;

/// <summary>
/// Consumer for handling updates to <see cref="UrlMultiAnalysis"/> entities,
/// processing incoming <see cref="UpdateMultiAnalysis{TAnalysis}"/> messages for <see cref="UrlAnalysis"/>.
/// </summary>
public class UpdateUrlMultiAnalysisConsumer
    : IConsumer<UpdateMultiAnalysis<UrlAnalysis>>
{
    private readonly IRepository<UrlMultiAnalysis, GlobalId> _repository;
    private readonly IMessageAnalysisUpdater _messageAnalysisUpdater;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUrlMultiAnalysisConsumer"/> class.
    /// </summary>
    /// <param name="repository">
    /// The repository for managing <see cref="UrlMultiAnalysis"/> entities,
    /// providing methods for retrieving and updating multi-analysis data.
    /// </param>
    /// <param name="messageAnalysisUpdater">
    /// An instance of <see cref="IMessageAnalysisUpdater"/> used to notify updates
    /// about child analysis states.
    /// </param>
    public UpdateUrlMultiAnalysisConsumer(
        IRepository<UrlMultiAnalysis, GlobalId> repository,
        IMessageAnalysisUpdater messageAnalysisUpdater)
    {
        _repository = repository;
        _messageAnalysisUpdater = messageAnalysisUpdater;
    }

    /// <inheritdoc/>
    public async Task Consume(
        ConsumeContext<UpdateMultiAnalysis<UrlAnalysis>> context)
    {
        UpdateMultiAnalysis<UrlAnalysis> message = context.Message;
        UrlMultiAnalysis? multiAnalysis = await _repository.GetAsync(
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

        await _repository.UpdateAsync(multiAnalysis, context.CancellationToken);

        await _messageAnalysisUpdater.NotifyChildAnalysisStateAsync(
            multiAnalysis.Id,
            context.CancellationToken);
    }
}