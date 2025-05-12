using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Domain.Common.ValueObjects;

/// <summary>
/// Represents the state of an analysis, including its status, verdict, and associated threat zone.
/// </summary>
public sealed record AnalysisState
{
    /// <summary>
    /// Gets the current status of the analysis.
    /// </summary>
    public AnalysisStatus Status { get; } = AnalysisStatus.Queued;

    /// <summary>
    /// Gets the verdict of the analysis.
    /// </summary>
    public Verdict Verdict { get; } = Verdict.Unknown;

    /// <summary>
    /// Gets the threat zone associated with the analysis.
    /// </summary>
    public ThreatZone ThreatZone { get; } = ThreatZone.Unknown;

    /// <summary>
    /// Gets a value indicating whether the analysis state can be updated.
    /// The state can be updated if the status is either Queued or InProgress.
    /// </summary>
    public bool CanBeUpdated => Status
        is AnalysisStatus.Queued
        or AnalysisStatus.InProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisState"/> class with the specified status, verdict, and threat zone.
    /// </summary>
    /// <param name="status">The status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="threatZone">The threat zone associated with the analysis.</param>
    private AnalysisState(
        AnalysisStatus status,
        Verdict verdict,
        ThreatZone threatZone)
    {
        Status = status;
        Verdict = verdict;
        ThreatZone = threatZone;
    }

    /// <summary>
    /// Creates and returns the initial state of an analysis.
    /// </summary>
    /// <returns>An <see cref="AnalysisState"/> instance with default values.</returns>
    public static AnalysisState Initial()
    {
        return new AnalysisState(
            AnalysisStatus.Queued,
            Verdict.Unknown,
            ThreatZone.Unknown);
    }

    /// <summary>
    /// Creates and returns an <see cref="AnalysisState"/> instance representing a failed analysis.
    /// </summary>
    /// <returns>An <see cref="AnalysisState"/> instance with a status of <see cref="AnalysisStatus.Failed"/>.</returns>
    public static AnalysisState CreateFailed()
    {
        return new AnalysisState(
            AnalysisStatus.Failed,
            Verdict.Unknown,
            ThreatZone.Unknown);
    }

    /// <summary>
    /// Creates a new instance of <see cref="AnalysisState"/> with an updated verdict.
    /// </summary>
    /// <param name="newVerdict">The new verdict to set for the analysis.</param>
    /// <returns>A new <see cref="AnalysisState"/> instance with the updated verdict and corresponding threat zone.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the analysis is already completed.</exception>
    public AnalysisState WithVerdict(Verdict newVerdict)
    {
        if (!CanBeUpdated)
        {
            throw new InvalidOperationException("Cannot update a completed, failed or timed out analysis.");
        }

        var newThreatZone = ThreatZoneMapping.Map[newVerdict];
        return new AnalysisState(Status, newVerdict, newThreatZone);
    }

    /// <summary>
    /// Creates a new instance of <see cref="AnalysisState"/> with an updated status.
    /// </summary>
    /// <param name="newStatus">The new status to set for the analysis.</param>
    /// <returns>A new <see cref="AnalysisState"/> instance with the updated status.</returns>
    public AnalysisState WithStatus(AnalysisStatus newStatus)
    {
        return new AnalysisState(newStatus, Verdict, ThreatZone);
    }
}