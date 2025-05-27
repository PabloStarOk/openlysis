using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.URLs.Entities;

/// <summary>
/// An analysis for a URL performed by an external service.
/// </summary>
/// <remarks>
/// This class inherits from the Entity class with a <see cref="AnalysisId"/> type parameter.
/// </remarks>
public sealed class UrlAnalysis : Analysis
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="threatScore">The threat score assigned by the service.</param>
    private UrlAnalysis(
        ComposedAnalysisId id,
        string serviceName,
        AnalysisState state,
        ThreatScore threatScore)
        : base(id, serviceName, state, threatScore)
    {
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private UrlAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="UrlAnalysis"/>.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="jobId">The job identifier. Optional.</param>
    /// <param name="threatScore">The threat score of the analysis. Optional.</param>
    /// <returns>A new instance of <see cref="UrlAnalysis"/>.</returns>
    public static UrlAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        string? jobId = null,
        ThreatScore? threatScore = null)
    {
        var composedId = ComposedAnalysisId.Create(id, jobId);
        var state = AnalysisState.Initial()
            .WithVerdict(verdict)
            .WithStatus(status);

        return new UrlAnalysis(
            composedId,
            serviceName,
            state,
            threatScore ?? ThreatScore.CreateNull());
    }

    /// <summary>
    /// Compares the current instance with another <see cref="UrlAnalysis"/> instance
    /// to determine if they have the same state.
    /// </summary>
    /// <param name="other">The other <see cref="UrlAnalysis"/> instance to compare with.</param>
    /// <returns>
    /// <c>true</c> if the current instance and the other instance have the same verdict,
    /// threat score, and status; otherwise, <c>false</c>.
    /// </returns>
    public bool HasSameStateTo(UrlAnalysis other)
    {
        return State == other.State;
    }
}