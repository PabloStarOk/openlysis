namespace Openlysis.AnalysisWorker.Configuration;

/// <summary>
/// Represents the options for the consumer in the file multi-analysis worker.
/// </summary>
public record AnalysisConsumerSettings
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "AnalysisRequestConsumer";

    /// <summary>
    /// Gets the update frequency in milliseconds.
    /// </summary>
    public int UpdateFrequencyMs { get; init; }
}