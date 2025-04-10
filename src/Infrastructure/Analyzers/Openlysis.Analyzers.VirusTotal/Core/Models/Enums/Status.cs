namespace Openlysis.Analyzers.VirusTotal.Core.Models.Enums;

/// <summary>
/// Represents the status of an analysis.
/// </summary>
public enum Status
{
    /// <summary>
    /// The analysis is queued.
    /// </summary>
    Queued,

    /// <summary>
    /// The analysis is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The analysis is completed.
    /// </summary>
    Completed,
}