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
    /// Initializes a new instance of the <see cref="UrlServiceAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="threatScore">The threat score assigned by the service.</param>
    private UrlServiceAnalysis(
        ComposedServiceAnalysisId id,
        string serviceName,
        AnalysisState state,
        ThreatScore threatScore)
        : base(id, serviceName, state, threatScore)
    {
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
        ThreatScore? threatScore = null)
    {
        var composedId = ComposedServiceAnalysisId.Create(id, jobId);
        var state = AnalysisState.Initial()
            .WithVerdict(verdict)
            .WithStatus(status);

        return new UrlServiceAnalysis(
            composedId,
            serviceName,
            state,
            threatScore ?? ThreatScore.CreateNull());
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
}