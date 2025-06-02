using System.ComponentModel.DataAnnotations;

namespace Openlysis.MultiAnalyzer.Configuration;

/// <summary>
/// Configuration options for the MultiAnalysis orchestrator, controlling the update batching and cycle intervals.
/// </summary>
internal sealed record OrchestrationOptions
{
    /// <summary>
    /// The configuration section name for binding these options.
    /// </summary>
    internal const string SectionName = "Orchestration";

    /// <summary>
    /// Gets the interval in milliseconds between each batch of update cycles.
    /// Controls how often the orchestrator processes a group of update cycles.
    /// </summary>
    [Range(1, int.MaxValue)]
    required public int UpdateBatchIntervalMs { get; init; }

    /// <summary>
    /// Gets the number of update cycles to execute in each batch.
    /// Determines how many times the orchestrator will attempt to update analyses per batch.
    /// </summary>
    [Range(1, int.MaxValue)]
    required public int UpdateCyclesPerBatch { get; init; }

    /// <summary>
    /// Gets the interval in milliseconds between each update cycle within a batch.
    /// Controls the delay between consecutive update cycles in a single batch.
    /// </summary>
    [Range(1, int.MaxValue)]
    required public int UpdateCycleIntervalMs { get; init; }
}
