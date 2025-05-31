using System.ComponentModel.DataAnnotations;

namespace Openlysis.MultiAnalyzer.Configuration;

/// <summary>
/// Represents the options for the consumer in the file multi-analysis worker.
/// </summary>
public record AnalyzeConsumerOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "Consumer";

    /// <summary>
    /// Gets the time in milliseconds to wait between batches to execute HTTP requests to external APIs.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int RequestBatchWaitTimeMs { get; init; }

    /// <summary>
    /// Gets the max HTTP requests to perform for each analyzer in a request batch.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int RequestsPerBatch { get; init; }

    /// <summary>
    /// Gets the value to know how frequent update requests are send.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int RequestFrequencyMs { get; init; }
}