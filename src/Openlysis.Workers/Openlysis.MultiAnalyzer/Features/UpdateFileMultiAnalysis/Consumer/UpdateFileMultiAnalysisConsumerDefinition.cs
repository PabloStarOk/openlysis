using MassTransit;

namespace Openlysis.MultiAnalyzer.Features.UpdateFileMultiAnalysis.Consumer;

/// <summary>
/// Defines the consumer for updating file multi-analysis.
/// </summary>
public class UpdateFileMultiAnalysisConsumerDefinition : ConsumerDefinition<UpdateFileMultiAnalysisConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFileMultiAnalysisConsumerDefinition"/> class.
    /// </summary>
    public UpdateFileMultiAnalysisConsumerDefinition()
    {
        EndpointName = UpdateFileMultiAnalysisConsumer.EndpointName;
    }
}