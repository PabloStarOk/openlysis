namespace Openlysis.Domain.Common.Enums;

/// <summary>
/// State of an analysis.
/// </summary>
public enum AnalysisStatus
{
    /// <summary>
    /// The analysis is in the queue.
    /// </summary>
    Queued,

    /// <summary>
    /// The analysis is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The analysis was finished.
    /// </summary>
    Finished,

    /// <summary>
    /// The analysis was not completed because of timeout.
    /// </summary>
    Timeout,
}
