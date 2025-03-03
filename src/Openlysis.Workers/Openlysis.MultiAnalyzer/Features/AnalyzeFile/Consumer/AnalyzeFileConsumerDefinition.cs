using MassTransit;

namespace Openlysis.MultiAnalyzer.Features.AnalyzeFile.Consumer;

/// <summary>
/// Defines the consumer for file multi-analysis.
/// </summary>
public class AnalyzeFileConsumerDefinition : ConsumerDefinition<AnalyzeFileConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumerDefinition"/> class.
    /// Sets the endpoint name to "file-multi-analysis".
    /// </summary>
    public AnalyzeFileConsumerDefinition()
    {
        EndpointName = AnalyzeFileConsumer.EndpointName;
    }
}