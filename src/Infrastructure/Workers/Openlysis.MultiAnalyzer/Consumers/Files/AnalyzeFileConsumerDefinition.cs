using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Messaging.Configuration;

namespace Openlysis.MultiAnalyzer.Consumers.Files;

/// <summary>
/// Defines the consumer for file multi-analysis.
/// </summary>
public class AnalyzeFileConsumerDefinition : ConsumerDefinition<AnalyzeFileConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumerDefinition"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options used to configure the endpoint name.</param>
    public AnalyzeFileConsumerDefinition(
        IOptions<BrokerSettings> brokerOptions)
    {
        EndpointName = brokerOptions.Value.AnalyzeFileEndpointName;
    }
}