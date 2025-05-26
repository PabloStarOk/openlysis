using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Entities;

/// <summary>
/// Defines a base entity to store an analysis from an external service.
/// </summary>
public abstract class ServiceAnalysis : Entity<ComposedServiceAnalysisId>
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
    /// Gets the threat score assigned by the service analysis, if available.
    /// </summary>
    public float? ThreatScore { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="threatScore">The threat score assigned by the service.</param>
    protected ServiceAnalysis(
        ComposedServiceAnalysisId id,
        string serviceName,
        AnalysisState state,
        float? threatScore)
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
    /// Initializes a new instance of the <see cref="ServiceAnalysis"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    protected ServiceAnalysis()
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
            throw new InvalidOperationException($"Trying to update verdict of {typeof(ServiceAnalysis)} when status is not queued or in-progress.");
        }

        State = State.WithVerdict(newVerdict);
    }

    /// <summary>
    /// Updates the threat score of the analysis, if the state allows it and the score is within the valid range.
    /// </summary>
    /// <param name="threatScore">
    /// The new threat score to set. Must be greater than 0.0 and less than 1.0, or null.
    /// </param>
    public void UpdateThreatScore(float? threatScore)
    {
        if (!State.CanBeUpdated)
        {
            throw new InvalidOperationException($"Trying to update threat score of {typeof(ServiceAnalysis)} when status is not queued or in-progress.");
        }

        if (threatScore is < .0f or > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(threatScore), "Threat score must be greater or equal than 0.0 and less or equal than 1.0, or null.");
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

    /// <summary>
    /// Normalizes the given threat score to ensure it falls within the range of 0.0 to 1.0.
    /// </summary>
    /// <param name="threatScore">The threat score to normalize. Can be null.</param>
    /// <returns>
    /// A normalized threat score between 0.0 and 1.0, or null if the input is null.
    /// If the input is greater than 1.0, it is divided by 100.0 before clamping.
    /// </returns>
    protected static float? NormalizeThreatScore(float? threatScore)
    {
        switch (threatScore)
        {
            case null:
                return null;
            case > 1.0f:
                threatScore /= 100.0f;
                break;
        }

        return Math.Clamp((float)threatScore, 0.0f, 1.0f);
    }
}