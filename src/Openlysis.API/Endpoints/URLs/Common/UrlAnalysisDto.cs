using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.API.Endpoints.URLs.Common;

/// <summary>
/// Data transfer object for <see cref="UrlAnalysis"/>.
/// </summary>
/// <param name="Id">The unique identifier of the URL service analysis.</param>
/// <param name="JobId">The optional job identifier related to the analysis.</param>
/// <param name="ServiceName">The name of the service that performed the analysis.</param>
/// <param name="Status">The current status of the analysis.</param>
/// <param name="Verdict">The verdict of the analysis.</param>
/// <param name="ThreatZone">The threat zone associated with the analysis.</param>
/// <param name="ThreatScore">The optional threat score of the analysis.</param>
public record UrlAnalysisDto(
    string Id,
    string? JobId,
    string ServiceName,
    AnalysisStatus Status,
    Verdict Verdict,
    ThreatZone ThreatZone,
    int? ThreatScore)
{
    /// <summary>
    /// Parses a <see cref="UrlAnalysis"/> object into a <see cref="UrlAnalysisDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="UrlAnalysis"/> object to parse.</param>
    /// <returns>A new <see cref="UrlAnalysisDto"/> object.</returns>
    public static UrlAnalysisDto Parse(UrlAnalysis source)
    {
        return new UrlAnalysisDto(
            source.Id.Primary.Value,
            source.Id.Job,
            source.ServiceName,
            source.State.Status,
            source.State.Verdict,
            source.State.ThreatZone,
            source.ThreatScore.NormalizedValue);
    }
}