namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;

/// <summary>
/// Represents the types of requests that can be made.
/// </summary>
public enum AnalysisEndpointType
{
    /// <summary>
    /// Request to analyze data.
    /// </summary>
    Analyze,

    /// <summary>
    /// Request to get the status of an analysis.
    /// </summary>
    GetStatus,

    /// <summary>
    /// Request to get the result of an analysis.
    /// </summary>
    GetResults,
}