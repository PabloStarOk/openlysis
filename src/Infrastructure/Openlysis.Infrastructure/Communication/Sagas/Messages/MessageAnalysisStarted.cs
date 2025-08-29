using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Communication.Sagas.Messages;

/// <summary>
/// Message indicating that analysis of a message has started.
/// </summary>
/// <param name="CorrelationId">The correlation identifier for the saga.</param>
/// <param name="MessageAnalysisId">The identifier for the message analysis.</param>
internal sealed record MessageAnalysisStarted(GlobalId CorrelationId, GlobalId MessageAnalysisId);