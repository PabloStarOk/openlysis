using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// Represents a generic request to update the analyses of a multi-analysis aggregate.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis to be updated, constrained to <see cref="Analysis"/>.</typeparam>
/// <param name="MultiAnalysisId">The unique identifier for the multi-analysis.</param>
/// <param name="UpdatableAnalyses">A collection of analyses to be updated.</param>
public record UpdateMultiAnalysis<TAnalysis>(
    GlobalId MultiAnalysisId,
    IEnumerable<TAnalysis> UpdatableAnalyses)
    where TAnalysis : Analysis;