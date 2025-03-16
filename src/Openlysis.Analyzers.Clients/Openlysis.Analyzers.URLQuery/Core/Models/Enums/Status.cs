namespace Openlysis.Analyzers.URLQuery.Core.Models.Enums;

/// <summary>
/// Represents the status of a query in the URLQuery client.
/// </summary>
internal enum Status
{
    /// <summary>
    /// The query is queued and waiting to be processed.
    /// </summary>
    Queued,

    /// <summary>
    /// The query is currently being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// The query is being analyzed.
    /// </summary>
    Analyzing,

    /// <summary>
    /// The query has been completed successfully.
    /// </summary>
    Done,

    /// <summary>
    /// The query has failed.
    /// </summary>
    Failed,
}