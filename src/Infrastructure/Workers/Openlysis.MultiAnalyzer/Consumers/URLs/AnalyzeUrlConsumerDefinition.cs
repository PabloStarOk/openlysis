using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Messaging.Configuration;

namespace Openlysis.MultiAnalyzer.Consumers.URLs;

/// <summary>
/// Defines the consumer for analyzing URLs.
/// </summary>
public class AnalyzeUrlConsumerDefinition : ConsumerDefinition<AnalyzeUrlConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlConsumerDefinition"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options used to configure the consumer.</param>
    public AnalyzeUrlConsumerDefinition(
        IOptions<BrokerSettings> brokerOptions)
    {
        EndpointName = brokerOptions.Value.AnalyzeUrlEndpointName;
    }
}