using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Communication.Consumers.Common;
using Openlysis.Infrastructure.Shared.Communication.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.Files;

/// <summary>
/// Defines the consumer for updating file multi-analysis.
/// </summary>
internal sealed class UpdateFileMultiAnalysisConsumerDefinition
    : ConsumerDefinition<UpdateMultiAnalysisConsumer<FileMultiAnalysis, FileAnalysis>>
{
    private readonly IOptions<ConsumersOptions> _consumersOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFileMultiAnalysisConsumerDefinition"/> class.
    /// </summary>
    /// <param name="consumersOptions">The options for configuring consumers.</param>
    public UpdateFileMultiAnalysisConsumerDefinition(IOptions<ConsumersOptions> consumersOptions)
    {
        EndpointName = consumersOptions.Value.UpdateFileAnalysis.Name;
        _consumersOptions = consumersOptions;
    }

    /// <inheritdoc/>
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<UpdateMultiAnalysisConsumer<FileMultiAnalysis, FileAnalysis>> consumerConfigurator,
        IRegistrationContext context)
    {
        ConsumersOptions options = _consumersOptions.Value;

        endpointConfigurator.ConcurrentMessageLimit = options.UpdateFileAnalysis.ConcurrencyLimit;
        endpointConfigurator.PrefetchCount = options.UpdateFileAnalysis.ConcurrencyLimit;

        endpointConfigurator.UseMessageRetry(r => r.Intervals(options.UpdateFileAnalysis.RetryIntervals));
        endpointConfigurator.UseInMemoryOutbox(context);

        IPartitioner partitioner = endpointConfigurator.CreatePartitioner(options.UpdateFileAnalysis.ConcurrencyLimit);
        endpointConfigurator.UsePartitioner<UpdateMultiAnalysisMessage<FileAnalysis>>(
            partitioner,
            consumeContext => consumeContext.Message.MultiAnalysisId.Value);
    }
}