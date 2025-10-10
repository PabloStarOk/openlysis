namespace Openlysis.Analyzers.Filescan.Core.Models.Enums;

/// <summary>
/// Represents the status of an analysis in the Filescan service.
/// </summary>
public enum Status
{
    /// <summary>
    /// The analysis has been created.
    /// </summary>
    Created,

    /// <summary>
    /// The analysis is queued and waiting to be processed.
    /// </summary>
    Queued,

    /// <summary>
    /// The analysis is currently being scanned.
    /// </summary>
    Scanning,

    /// <summary>
    /// The analysis has been finished.
    /// </summary>
    Finished,
}