using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.URLs.Entities;

/// <summary>
/// An analysis for a URL performed by an external service.
/// </summary>
public sealed class UrlAnalysis : Analysis
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="externalId">The identifiers assigned by the external analysis service.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="threatScore">The threat score assigned by the service.</param>
    private UrlAnalysis(
        GlobalId id,
        ExternalAnalysisId externalId,
        string serviceName,
        AnalysisState state,
        ThreatScore threatScore)
        : base(id, externalId, serviceName, state, threatScore)
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
    /// <param name="externalPrimaryId">The primary identifier assigned by the external service.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="externalJobId">An optional job identifier assigned by the external service.</param>
    /// <param name="threatScore">The threat score of the analysis. Optional.</param>
    /// <returns>A new instance of <see cref="UrlAnalysis"/>.</returns>
    public static UrlAnalysis Create(
        string externalPrimaryId,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        string? externalJobId = null,
        ThreatScore? threatScore = null)
    {
        var state = AnalysisState.Initial()
            .WithVerdict(verdict)
            .WithStatus(status);

        return new UrlAnalysis(
            GlobalId.CreateUnique(),
            ExternalAnalysisId.Create(externalPrimaryId, externalJobId),
            serviceName,
            state,
            threatScore ?? ThreatScore.CreateNull());
    }

    /// <summary>
    /// Creates a new instance of <see cref="UrlAnalysis"/> with a specified <see cref="GlobalId"/>.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="externalId">The identifiers assigned by the external analysis service.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="threatScore">The threat score of the analysis. Optional.</param>
    /// <returns>A new instance of <see cref="UrlAnalysis"/>.</returns>
    public static UrlAnalysis CreateWithId(
        GlobalId id,
        ExternalAnalysisId externalId,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        ThreatScore? threatScore = null)
    {
        var state = AnalysisState.Initial()
            .WithVerdict(verdict)
            .WithStatus(status);

        return new UrlAnalysis(
            id,
            externalId,
            serviceName,
            state,
            threatScore ?? ThreatScore.CreateNull());
    }
}