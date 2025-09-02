namespace Openlysis.MultiAnalyzer.Communication.Consumers.Common;

/// <summary>
/// Represents the status of an analysis job consumer.
/// </summary>
internal enum AnalysisJobConsumerStatus
{
    /// <summary>
    /// The consumer has started processing the job.
    /// </summary>
    Started = 1,

    /// <summary>
    /// The consumer is reporting the start of the job.
    /// </summary>
    ReportingStart = 2,

    /// <summary>
    /// The consumer is polling for job updates.
    /// </summary>
    Polling = 3,

    /// <summary>
    /// The consumer has completed processing the job.
    /// </summary>
    Completed = 4,
}