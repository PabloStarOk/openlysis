using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Messaging.Configuration;

namespace Openlysis.Infrastructure.Services.Messaging.Consumers.Files;

/// <summary>
/// Defines the consumer for updating file multi-analysis.
/// </summary>
public class UpdateFileMultiAnalysisConsumerDefinition : ConsumerDefinition<UpdateFileMultiAnalysisConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFileMultiAnalysisConsumerDefinition"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options used to configure the endpoint name.</param>
    public UpdateFileMultiAnalysisConsumerDefinition(
        IOptions<BrokerSettings> brokerOptions)
    {
        EndpointName = brokerOptions.Value.UpdateFileAnalysisEndpointName;
    }
}