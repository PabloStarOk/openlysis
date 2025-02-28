using MassTransit;

namespace Openlysis.FileMultiAnalysisWorker.Consumer;

/// <summary>
/// Defines the consumer for file multi-analysis.
/// </summary>
public class FileMultiAnalysisConsumerDefinition : ConsumerDefinition<FileMultiAnalysisConsumer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisConsumerDefinition"/> class.
    /// Sets the endpoint name to "file-multi-analysis".
    /// </summary>
    public FileMultiAnalysisConsumerDefinition()
    {
        EndpointName = "file-multi-analysis";
    }
}