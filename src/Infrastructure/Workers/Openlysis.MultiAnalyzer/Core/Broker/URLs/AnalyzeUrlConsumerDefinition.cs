using MassTransit;

using Openlysis.MultiAnalyzer.Core.Broker.Files;

namespace Openlysis.MultiAnalyzer.Core.Broker.URLs;

/// <summary>
/// Defines the consumer for analyzing URLs.
/// </summary>
public class AnalyzeUrlConsumerDefinition : ConsumerDefinition<AnalyzeUrlConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlConsumerDefinition"/> class.
    /// Sets the endpoint name to the same as <see cref="AnalyzeFileConsumer"/>.
    /// </summary>
    public AnalyzeUrlConsumerDefinition()
    {
        EndpointName = AnalyzeUrlConsumer.EndpointName;
    }
}