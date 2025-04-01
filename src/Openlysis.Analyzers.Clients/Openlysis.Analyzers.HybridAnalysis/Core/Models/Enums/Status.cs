namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

/// <summary>
/// Represents the status of a sandbox analysis.
/// </summary>
public enum Status
{
    /// <summary>
    /// The analysis is in the queue.
    /// </summary>
    InQueue,

    /// <summary>
    /// The analysis is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The analysis completed with partial success.
    /// </summary>
    PartialSuccess,

    /// <summary>
    /// The analysis completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The analysis couldn't be completed, an error occurred.
    /// </summary>
    Error,
}