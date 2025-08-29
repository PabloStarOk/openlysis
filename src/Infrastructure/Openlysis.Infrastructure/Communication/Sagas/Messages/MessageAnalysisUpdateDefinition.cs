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
    private readonly IOptions<BrokerSettings> _brokerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisUpdateDefinition"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options.</param>
    public MessageAnalysisUpdateDefinition(IOptions<BrokerSettings> brokerOptions)
    {
        EndpointName = brokerOptions.Value.MessageAnalysisUpdateEndpointName;
        _brokerOptions = brokerOptions;
    }

    /// <inheritdoc/>
    protected override void ConfigureSaga(
        IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<MessageAnalysisUpdateSaga> sagaConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.ConcurrentMessageLimit = _brokerOptions.Value.MessageAnalysisUpdateConcurrencyLimit;

        endpointConfigurator.UseMessageRetry(r => r.Intervals(_brokerOptions.Value.MessageAnalysisUpdateRetryIntervals));
        endpointConfigurator.UseInMemoryOutbox(context);

        IPartitioner partitioner = endpointConfigurator.CreatePartitioner(_brokerOptions.Value.MessageAnalysisUpdateConcurrencyLimit);
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