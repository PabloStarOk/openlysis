using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Messaging.Configuration;

namespace Openlysis.Infrastructure.Services.Messaging.Consumers.URLs;

/// <summary>
/// Defines the consumer for updating URL multi-analysis.
/// </summary>
public class UpdateUrlMultiAnalysisConsumerDefinition : ConsumerDefinition<UpdateUrlMultiAnalysisConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUrlMultiAnalysisConsumerDefinition"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options used to configure the endpoint name.</param>
    public UpdateUrlMultiAnalysisConsumerDefinition(
        IOptions<BrokerSettings> brokerOptions)
    {
        EndpointName = brokerOptions.Value.UpdateUrlAnalysisEndpointName;
    }
}