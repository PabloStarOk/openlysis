namespace Openlysis.Application.Common.Models;

/// <summary>
/// Represents the status of an analysis request.
/// </summary>
public enum AnalysisRequestStatus
{
    /// <summary>
    /// The analysis has been queued to be started.
    /// </summary>
    Queued,

    /// <summary>
    /// The analysis has been retrieved.
    /// </summary>
    Retrieved,
}