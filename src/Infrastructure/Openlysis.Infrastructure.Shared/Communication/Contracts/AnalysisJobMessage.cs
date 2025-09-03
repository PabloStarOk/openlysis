using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Shared.Communication.Contracts;

/// <summary>
/// Represents a message for an analysis job, containing the multi-analysis identifier and an optional correlation identifier.
/// </summary>
/// <param name="MultiAnalysisId">The unique identifier for the multi-analysis job.</param>
/// <param name="CorrelationId">An optional correlation identifier that associates the request to a message analysis.</param>
public record AnalysisJobMessage(GlobalId MultiAnalysisId, GlobalId? CorrelationId)
    : QueueMessage(CorrelationId);