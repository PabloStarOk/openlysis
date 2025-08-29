using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// Represents a message to request a multi-analysis update with the given analyses.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis to be updated. Must inherit from <see cref="Analysis"/>.</typeparam>
/// <param name="MultiAnalysisId">The unique identifier for the multi-analysis aggregate to be updated.</param>
/// <param name="Timeout">Indicates whether the update is due to a timeout.</param>
/// <param name="UpdatableAnalyses">A collection of analyses to be updated within the multi-analysis aggregate.</param>
/// <param name="CorrelationId">Optional correlation identifier that associates the multi-analysis to a message analysis.</param>
public record UpdateMultiAnalysisMessage<TAnalysis>(
    GlobalId MultiAnalysisId,
    bool Timeout,
    IEnumerable<TAnalysis> UpdatableAnalyses,
    GlobalId? CorrelationId)
    : QueueMessage(CorrelationId)
    where TAnalysis : Analysis;