using MassTransit;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.Common;

/// <summary>
/// MassTransit consumer that handles <see cref="UpdateMultiAnalysisMessage{TAnalysis}"/> messages,
/// updating or adding analyses to a <typeparamref name="TMultiAnalysis"/> aggregate.
/// </summary>
/// <typeparam name="TMultiAnalysis">The multi-analysis aggregate type.</typeparam>
/// <typeparam name="TAnalysis">The analysis entity type.</typeparam>
internal sealed class UpdateMultiAnalysisConsumer<TMultiAnalysis, TAnalysis>
    : IConsumer<UpdateMultiAnalysisMessage<TAnalysis>>
    where TMultiAnalysis : MultiAnalysis<TAnalysis>
    where TAnalysis : Analysis
{
    private readonly IRepository<TMultiAnalysis, GlobalId> _repository;
    private readonly IEndpointUriProvider _endpointUriProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMultiAnalysisConsumer{TMultiAnalysis, TAnalysis}"/> class.
    /// </summary>
    /// <param name="repository">The repository for accessing and updating the multi-analysis aggregate.</param>
    /// <param name="endpointUriProvider">The provider for endpoint URIs used in message communication.</param>
    public UpdateMultiAnalysisConsumer(
        IRepository<TMultiAnalysis, GlobalId> repository,
        IEndpointUriProvider endpointUriProvider)
    {
        _repository = repository;
        _endpointUriProvider = endpointUriProvider;
    }

    /// <inheritdoc/>
    public async Task Consume(
        ConsumeContext<UpdateMultiAnalysisMessage<TAnalysis>> context)
    {
        UpdateMultiAnalysisMessage<TAnalysis> message = context.Message;
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
        if (message.CorrelationId is not null)
        {
            await context.Send(_endpointUriProvider.MessageAnalysisUpdateUri, message);
        }
    }
}