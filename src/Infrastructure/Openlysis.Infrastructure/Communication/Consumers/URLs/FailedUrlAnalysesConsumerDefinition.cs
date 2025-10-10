using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Communication.Consumers.Common;
using Openlysis.Infrastructure.Shared.Communication.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.URLs;

/// <summary>
/// Consumer definition for handling faulted URL multi-analysis jobs.
/// </summary>
internal sealed class FailedUrlAnalysesConsumerDefinition
    : ConsumerDefinition<FaultedMultiAnalysisConsumer<UrlAnalysis, UrlAnalysisJobMessage>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailedUrlAnalysesConsumerDefinition"/> class.
    /// </summary>
    /// <param name="options">The consumer options containing endpoint configuration.</param>
    public FailedUrlAnalysesConsumerDefinition(IOptions<ConsumersOptions> options)
    {
        EndpointName = options.Value.AnalyzeUrl.ErrorName;
    }

    /// <inheritdoc/>
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<FaultedMultiAnalysisConsumer<UrlAnalysis, UrlAnalysisJobMessage>> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.DiscardSkippedMessages();
    }
}