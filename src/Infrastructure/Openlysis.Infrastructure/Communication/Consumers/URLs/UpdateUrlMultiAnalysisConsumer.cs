using MassTransit;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Models;

namespace Openlysis.Infrastructure.Communication.Consumers.URLs;

/// <summary>
/// Consumer class for handling the update of <see cref="UrlMultiAnalysis"/> with incoming <see cref="UrlServiceAnalysis"/> objects.
/// </summary>
/// <remarks>
/// This class consumes messages of type <see cref="UpdateUrlMultiAnalysis"/> and updates the corresponding URL multi-analysis in the repository.
/// </remarks>
public class UpdateUrlMultiAnalysisConsumer : IConsumer<UpdateUrlMultiAnalysis>
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
    public async Task Consume(ConsumeContext<UpdateUrlMultiAnalysis> context)
    {
        UrlServiceAnalysis[] serviceAnalyses = context.Message.Analyses;
        UrlMultiAnalysis? multiAnalysis = await _repository.GetAsync(
            context.Message.MultiAnalysisId,
            context.CancellationToken);
        ArgumentNullException.ThrowIfNull(multiAnalysis);

        foreach (var analysis in serviceAnalyses)
        {
            if (multiAnalysis.ServiceAnalyses.Contains(analysis))
            {
                multiAnalysis.UpdateServiceAnalysis(analysis);
                continue;
            }

            multiAnalysis.AddServiceAnalysis(analysis);
        }

        await _repository.UpdateAsync(multiAnalysis, context.CancellationToken);

        await _messageAnalysisUpdater.NotifyChildAnalysisStateAsync(
            multiAnalysis.Id,
            context.CancellationToken);
    }
}