using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Sagas.Messages;

/// <summary>
/// Saga definition for handling message analysis update workflows.
/// Configures the saga endpoint and concurrency settings using broker options.
/// </summary>
internal sealed class MessageAnalysisUpdateDefinition : SagaDefinition<MessageAnalysisUpdateSaga>
{
    private readonly IOptions<ConsumersOptions> _consumersOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisUpdateDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">The options for configuring consumers.</param>
    public MessageAnalysisUpdateDefinition(IOptions<ConsumersOptions> consumersOptions)
    {
        EndpointName = consumersOptions.Value.MessageAnalysisUpdate.Name;
        _consumersOptions = consumersOptions;
    }

    /// <inheritdoc/>
    protected override void ConfigureSaga(
        IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<MessageAnalysisUpdateSaga> sagaConfigurator,
        IRegistrationContext context)
    {
        ConsumersOptions options = _consumersOptions.Value;
        endpointConfigurator.ConcurrentMessageLimit = options.MessageAnalysisUpdate.ConcurrencyLimit;

        endpointConfigurator.UseMessageRetry(r => r.Intervals(options.MessageAnalysisUpdate.RetryIntervals));
        endpointConfigurator.UseInMemoryOutbox(context);

        IPartitioner partitioner = endpointConfigurator.CreatePartitioner(options.MessageAnalysisUpdate.ConcurrencyLimit);
        sagaConfigurator.Message<MessageAnalysisStarted>(
            x => x.UsePartitioner(partitioner, m => m.Message.CorrelationId.Value));
        sagaConfigurator.Message<MessageAnalysisInitialized>(
            x => x.UsePartitioner(partitioner, m => m.Message.CorrelationId.Value));
        sagaConfigurator.Message<UpdateMultiAnalysisMessage<FileAnalysis>>(
            x => x.UsePartitioner(partitioner, m => m.Message.CorrelationId?.Value ?? Guid.Empty));
        sagaConfigurator.Message<UpdateMultiAnalysisMessage<UrlAnalysis>>(
            x => x.UsePartitioner(partitioner, m => m.Message.CorrelationId?.Value ?? Guid.Empty));
    }
}