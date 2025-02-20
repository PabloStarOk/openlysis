namespace Openlysis.Domain.Common.Enums;

/// <summary>
/// State of a scan.
/// </summary>
public enum AnalysisStatus
{
    /// <summary>
    /// The scan is in the queue.
    /// </summary>
    Queued,

    /// <summary>
    /// The scan was started.
    /// </summary>
    Started,

    /// <summary>
    /// The scan was finished.
    /// </summary>
    Finished,

    /// <summary>
    /// The scan was not completed because of timeout.
    /// </summary>
    Timeout,
}
