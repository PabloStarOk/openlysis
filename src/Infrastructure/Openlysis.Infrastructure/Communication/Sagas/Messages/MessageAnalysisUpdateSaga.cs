using MassTransit;

using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Communication.Sagas.Messages;

/// <summary>
/// Represents the saga state for updating message analysis.
/// </summary>
internal sealed record MessageAnalysisUpdateSaga : SagaStateMachineInstance
{
    /// <summary>
    /// Gets or sets the correlation identifier for the saga instance.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets the current state of the saga.
    /// </summary>
    public int CurrentState { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the message analysis being updated.
    /// </summary>
    required public GlobalId MessageAnalysisId { get; set; }

    /// <summary>
    /// Gets the set of deferred file updates associated with the message analysis.
    /// </summary>
    public HashSet<GlobalId> DeferredFileUpdates { get; init; } = [];

    /// <summary>
    /// Gets the set of deferred URL updates associated with the message analysis.
    /// </summary>
    public HashSet<GlobalId> DeferredUrlUpdates { get; init; } = [];
}