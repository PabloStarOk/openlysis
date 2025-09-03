using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Analyzers.Shared.Contracts.Common.Models;

/// <summary>
/// Represents the identity of an analysis, including its internal ID used across the system and external identifiers.
/// </summary>
/// <param name="Id">The internally unique identifier for the analysis.</param>
/// <param name="ExternalId">The external identifier associated with the analysis.</param>
public sealed record AnalysisIdentity(
    GlobalId Id,
    ExternalAnalysisId ExternalId)
{
    /// <summary>
    /// Creates an <see cref="AnalysisIdentity"/> from an <see cref="Analysis"/> entity.
    /// </summary>
    /// <param name="analysis">The analysis entity to extract identity information from.</param>
    /// <returns>An <see cref="AnalysisIdentity"/> containing the internal and external IDs.</returns>
    public static AnalysisIdentity FromAnalysis(Analysis analysis)
    {
        return new AnalysisIdentity(analysis.Id, analysis.ExternalId);
    }
}