namespace Openlysis.Analyzers.Shared.Contracts.Common.Requests;

/// <summary>
/// Defines a base request to analyze content.
/// </summary>
/// <param name="Description">A description of the analysis request.</param>
/// <param name="IsPrivate">Indicates whether the request is private.</param>
public abstract record AnalyzeRequest(
    string? Description = "",
    bool IsPrivate = false);