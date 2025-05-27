using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Entities;

/// <summary>
/// Defines a base entity to store an analysis from an external service.
/// </summary>
public abstract class Analysis : Entity<ComposedAnalysisId>
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the current state of the analysis.
    /// </summary>
    public AnalysisState State { get; private set; }

    /// <summary>
    /// Gets the threat score assigned by the service, if available.
    /// </summary>
    public ThreatScore ThreatScore { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Analysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="threatScore">The threat score assigned by the service.</param>
    protected Analysis(
        ComposedAnalysisId id,
        string serviceName,
        AnalysisState state,
        ThreatScore threatScore)
        : base(id)
    {
        ServiceName = serviceName;
        State = state;
        ThreatScore = threatScore;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="Analysis"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    protected Analysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Updates the verdict of the analysis.
    /// </summary>
    /// <param name="newVerdict">The new verdict to set.</param>
    /// <remarks>
    /// The verdict can only be updated if the status is either Queued or InProgress.
    /// </remarks>
    public void UpdateVerdict(Verdict newVerdict)
    {
        if (!State.CanBeUpdated)
        {
            throw new InvalidOperationException($"Trying to update verdict of {typeof(Analysis)} when status is not queued or in-progress.");
        }

        State = State.WithVerdict(newVerdict);
    }

    /// <summary>
    /// Updates the threat score of the analysis.
    /// </summary>
    /// <param name="threatScore">The new threat score.</param>
    public void UpdateThreatScore(ThreatScore threatScore)
    {
        ArgumentNullException.ThrowIfNull(threatScore);
        if (!State.CanBeUpdated)
        {
            throw new InvalidOperationException($"Trying to update threat score of {typeof(Analysis)} when status is not queued or in-progress.");
        }

        ThreatScore = threatScore;
    }

    /// <summary>
    /// Updates the status of the analysis.
    /// </summary>
    /// <param name="newStatus">The new status to set.</param>
    public void UpdateStatus(AnalysisStatus newStatus)
    {
        ArgumentNullException.ThrowIfNull(newStatus);
        State = State.WithStatus(newStatus);
    }
}