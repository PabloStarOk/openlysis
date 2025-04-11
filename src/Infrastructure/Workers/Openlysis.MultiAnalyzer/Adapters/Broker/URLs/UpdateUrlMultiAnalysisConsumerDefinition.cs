using MassTransit;

namespace Openlysis.MultiAnalyzer.Adapters.Broker.URLs;

/// <summary>
/// Defines the consumer for updating URL multi-analysis.
/// </summary>
public class UpdateUrlMultiAnalysisConsumerDefinition : ConsumerDefinition<UpdateUrlMultiAnalysisConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUrlMultiAnalysisConsumerDefinition"/> class.
    /// Sets the endpoint name for the consumer.
    /// </summary>
    public UpdateUrlMultiAnalysisConsumerDefinition()
    {
        EndpointName = UpdateUrlMultiAnalysisConsumer.EndpointName;
    }
}