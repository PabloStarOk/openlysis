using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Communication.Consumers.Common;
using Openlysis.Infrastructure.Shared.Communication.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.Files;

/// <summary>
/// Consumer definition for handling faulted file multi-analysis jobs.
/// </summary>
internal sealed class FailedFileAnalysesConsumerDefinition
    : ConsumerDefinition<FaultedMultiAnalysisConsumer<FileAnalysis, FileAnalysisJobMessage>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailedFileAnalysesConsumerDefinition"/> class.
    /// </summary>
    /// <param name="options">The consumer options containing endpoint configuration.</param>
    public FailedFileAnalysesConsumerDefinition(IOptions<ConsumersOptions> options)
    {
        EndpointName = options.Value.AnalyzeFile.ErrorName;
    }

    /// <inheritdoc/>
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<FaultedMultiAnalysisConsumer<FileAnalysis, FileAnalysisJobMessage>> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.DiscardSkippedMessages();
    }
}