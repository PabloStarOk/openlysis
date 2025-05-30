using MassTransit;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.Files;

/// <summary>
/// MassTransit consumer that handles updates to <see cref="FileMultiAnalysis"/> entities
/// by processing <see cref="UpdateMultiAnalysis{TAnalysis}"/> messages containing <see cref="FileAnalysis"/> updates.
/// </summary>
public class UpdateFileMultiAnalysisConsumer
    : IConsumer<UpdateMultiAnalysis<FileAnalysis>>
{
    private readonly IRepository<FileMultiAnalysis, GlobalId> _repository;
    private readonly IMessageAnalysisUpdater _messageAnalysisUpdater;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFileMultiAnalysisConsumer"/> class.
    /// </summary>
    /// <param name="repository">
    /// An instance of <see cref="IRepository{TModel,TModelId}"/> used to manage
    /// <see cref="FileMultiAnalysis"/> entities in the persistence layer.
    /// </param>
    /// <param name="messageAnalysisUpdater">
    /// An instance of <see cref="IMessageAnalysisUpdater"/> used to notify updates
    /// about child analysis states.
    /// </param>
    public UpdateFileMultiAnalysisConsumer(
        IRepository<FileMultiAnalysis, GlobalId> repository,
        IMessageAnalysisUpdater messageAnalysisUpdater)
    {
        _repository = repository;
        _messageAnalysisUpdater = messageAnalysisUpdater;
    }

    /// <inheritdoc/>
    public async Task Consume(
        ConsumeContext<UpdateMultiAnalysis<FileAnalysis>> context)
    {
        UpdateMultiAnalysis<FileAnalysis> message = context.Message;
        FileMultiAnalysis? fileMultiAnalysis = await _repository.GetAsync(
                message.MultiAnalysisId,
                context.CancellationToken);
        ArgumentNullException.ThrowIfNull(fileMultiAnalysis);

        foreach (var serviceAnalysis in message.UpdatableAnalyses)
        {
            if (fileMultiAnalysis.Analyses.Contains(serviceAnalysis))
            {
                fileMultiAnalysis.UpdateAnalysis(serviceAnalysis);
                continue;
            }

            fileMultiAnalysis.AddAnalysis(serviceAnalysis);
        }

        await _repository.UpdateAsync(
            fileMultiAnalysis,
            context.CancellationToken);

        await _messageAnalysisUpdater.NotifyChildAnalysisStateAsync(
            fileMultiAnalysis.Id,
            context.CancellationToken);
    }
}