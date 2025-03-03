namespace Openlysis.MultiAnalyzer.Infrastructure.Configuration;

/// <summary>
/// Represents the options for the consumer in the file multi-analysis worker.
/// </summary>
public record AnalyzeFileConsumerSettings
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "AnalyzeFileConsumer";

    /// <summary>
    /// Gets the time in milliseconds to wait between batches to execute HTTP requests to external APIs.
    /// </summary>
    public int RequestBatchWaitTimeMs { get; init; }

    /// <summary>
    /// Gets the max HTTP requests to perform for each analyzer in a request batch.
    /// </summary>
    public int MaxRequestsPerBatch { get; init; }

    /// <summary>
    /// Gets the update analysis frequency in milliseconds.
    /// </summary>
    public int RequestFrequencyMs { get; init; }
}