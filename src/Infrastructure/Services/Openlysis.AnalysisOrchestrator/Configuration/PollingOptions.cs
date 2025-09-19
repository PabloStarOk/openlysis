using System.ComponentModel.DataAnnotations;

namespace Openlysis.AnalysisOrchestrator.Configuration;

/// <summary>
/// Represents configuration options for analysis polling operations.
/// </summary>
internal sealed record PollingOptions
{
    /// <summary>
    /// The configuration section name for polling options.
    /// </summary>
    internal const string SectionName = "Polling";

    /// <summary>
    /// Gets the interval in milliseconds between batches.
    /// Must be at least 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    required public int BatchIntervalMs { get; init; }

    /// <summary>
    /// Gets the number of cycles per batch.
    /// Must be at least 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    required public int CyclesPerBatch { get; init; }

    /// <summary>
    /// Gets the interval in milliseconds between cycles.
    /// Must be at least 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    required public int CycleIntervalMs { get; init; }
}
