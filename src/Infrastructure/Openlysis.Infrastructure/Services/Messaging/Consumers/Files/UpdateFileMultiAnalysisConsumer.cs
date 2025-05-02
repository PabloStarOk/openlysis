using MassTransit;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Infrastructure.Shared.Messaging.Models;

namespace Openlysis.Infrastructure.Services.Messaging.Consumers.Files;

/// <summary>
/// Consumer class for handling the UpdateFileMultiAnalysis message.
/// </summary>
/// <remarks>
/// This class consumes messages of type <see cref="UpdateFileMultiAnalysis"/> and processes them.
/// </remarks>
public class UpdateFileMultiAnalysisConsumer : IConsumer<UpdateFileMultiAnalysis>
{
    private readonly IRepository<FileMultiAnalysis, GlobalId> _multiAnalysisRepository;
    private readonly IMessageAnalysisUpdater _messageAnalysisUpdater;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFileMultiAnalysisConsumer"/> class.
    /// </summary>
    /// <param name="multiAnalysisRepository">
    /// An instance of <see cref="IRepository{TModel,TModelId}"/> used to manage
    /// <see cref="FileMultiAnalysis"/> entities in the persistence layer.
    /// </param>
    /// <param name="messageAnalysisUpdater">
    /// An instance of <see cref="IMessageAnalysisUpdater"/> used to notify updates
    /// about child analysis states.
    /// </param>
    public UpdateFileMultiAnalysisConsumer(
        IRepository<FileMultiAnalysis, GlobalId> multiAnalysisRepository,
        IMessageAnalysisUpdater messageAnalysisUpdater)
    {
        _multiAnalysisRepository = multiAnalysisRepository;
        _messageAnalysisUpdater = messageAnalysisUpdater;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<UpdateFileMultiAnalysis> context)
    {
        UpdateFileMultiAnalysis request = context.Message;
        FileMultiAnalysis? fileMultiAnalysis = await _multiAnalysisRepository.GetAsync(request.Id);
        ArgumentNullException.ThrowIfNull(fileMultiAnalysis);

        foreach (var serviceAnalysis in request.ServiceFileAnalyses)
        {
            if (fileMultiAnalysis.ServiceAnalyses.Contains(serviceAnalysis))
            {
                fileMultiAnalysis.UpdateServiceAnalysis(serviceAnalysis);
                continue;
            }

            fileMultiAnalysis.AddServiceAnalysis(serviceAnalysis);
        }

        await _multiAnalysisRepository.UpdateAsync(
            fileMultiAnalysis,
            context.CancellationToken);

        await _messageAnalysisUpdater.NotifyChildAnalysisStateAsync(
            fileMultiAnalysis.Id,
            context.CancellationToken);
    }
}