using System.ComponentModel.DataAnnotations;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.URLs.Entities;

/// <summary>
/// Represents an analysis of a URL service.
/// </summary>
/// <remarks>
/// This class inherits from the Entity class with a <see cref="ServiceAnalysisId"/> type parameter.
/// </remarks>
public sealed class UrlServiceAnalysis : ServiceAnalysis
{
    /// <summary>
    /// Gets the threat score of the analysis.
    /// </summary>
    [Range(.0f, 1.0f)]
    public float? ThreatScore { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlServiceAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="threatScore">The threat score of the analysis. Optional.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="error">The error message if the analysis failed.</param>
    private UrlServiceAnalysis(
        ComposedServiceAnalysisId id,
        string serviceName,
        AnalysisState state,
        float? threatScore,
        string? error)
        : base(id, serviceName, state, error)
    {
        ThreatScore = threatScore;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private UrlServiceAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="UrlServiceAnalysis"/>.
    /// </summary>
    /// <param name="id">The unique identifier for the service analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="jobId">The job identifier. Optional.</param>
    /// <param name="threatScore">The threat score of the analysis. Optional.</param>
    /// <returns>A new instance of <see cref="UrlServiceAnalysis"/>.</returns>
    public static UrlServiceAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        string? jobId = null,
        float? threatScore = null)
    {
        var composedId = ComposedServiceAnalysisId.Create(id, jobId);
        float? normalizedThreatScore = NormalizeThreatScore(threatScore);
        var state = AnalysisState.Initial()
            .WithVerdict(verdict)
            .WithStatus(status);

        return new UrlServiceAnalysis(
            composedId,
            serviceName,
            state,
            normalizedThreatScore,
            error: null);
    }

    /// <summary>
    /// Creates a new instance of <see cref="UrlServiceAnalysis"/> with Failed status.
    /// </summary>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="error">The error message indicating why the analysis failed.</param>
    /// <returns>A new instance of <see cref="UrlServiceAnalysis"/> with Failed status, Unknown verdict, and Unknown threat zone.</returns>
    /// <remarks>
    /// This method is used when an analysis has failed and creates an analysis with empty IDs and appropriate failure indicators.
    /// </remarks>
    public static UrlServiceAnalysis CreateFailed(
        string serviceName,
        string error)
    {
        string id = string.Empty;
        string jobId = string.Empty;
        var composedId = ComposedServiceAnalysisId.Create(id, jobId);
        return new UrlServiceAnalysis(
            composedId,
            serviceName,
            AnalysisState.CreateFailed(),
            null,
            error: error);
    }

    /// <summary>
    /// Updates the threat score of the analysis.
    /// </summary>
    /// <param name="threatScore">The new threat score to set.</param>
    /// <remarks>
    /// The threat score must be between 0.0 and 1.0.
    /// </remarks>
    public void UpdateThreatScore(float? threatScore)
    {
        if (State.Status
            is not AnalysisStatus.Queued
            and not AnalysisStatus.InProgress)
        {
            throw new InvalidOperationException("Trying to update threat score of the UrlServiceAnalysis when analysis status is not queued or in-progress.");
        }

        if (threatScore is > .0f and < 1.0f)
        {
            ThreatScore = threatScore;
        }
    }

    /// <summary>
    /// Compares the current instance with another <see cref="UrlServiceAnalysis"/> instance
    /// to determine if they have the same state.
    /// </summary>
    /// <param name="other">The other <see cref="UrlServiceAnalysis"/> instance to compare with.</param>
    /// <returns>
    /// <c>true</c> if the current instance and the other instance have the same verdict,
    /// threat score, and status; otherwise, <c>false</c>.
    /// </returns>
    public bool HasSameStateTo(UrlServiceAnalysis other)
    {
        return State == other.State;
    }

    /// <summary>
    /// Normalizes the given threat score to ensure it falls within the range of 0.0 to 1.0.
    /// </summary>
    /// <param name="threatScore">The threat score to normalize. Can be null.</param>
    /// <returns>
    /// A normalized threat score between 0.0 and 1.0, or null if the input is null.
    /// If the input is greater than 1.0, it is divided by 100.0 before clamping.
    /// </returns>
    private static float? NormalizeThreatScore(float? threatScore)
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