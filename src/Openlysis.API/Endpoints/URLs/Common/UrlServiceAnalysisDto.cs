using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.API.Endpoints.URLs.Common;

/// <summary>
/// Data transfer object for <see cref="UrlServiceAnalysis"/>.
/// </summary>
/// <param name="Id">The unique identifier of the URL service analysis.</param>
/// <param name="JobId">The optional job identifier related to the analysis.</param>
/// <param name="ServiceName">The name of the service that performed the analysis.</param>
/// <param name="Status">The current status of the analysis.</param>
/// <param name="Verdict">The verdict of the analysis.</param>
/// <param name="ThreatZone">The threat zone associated with the analysis.</param>
/// <param name="ThreatScore">The optional threat score of the analysis.</param>
/// <param name="Error">The error message if the analysis failed.</param>
public record UrlServiceAnalysisDto(
    string Id,
    string? JobId,
    string ServiceName,
    AnalysisStatus Status,
    Verdict Verdict,
    ThreatZone ThreatZone,
    float? ThreatScore,
    string? Error)
{
    /// <summary>
    /// Parses a <see cref="UrlServiceAnalysis"/> object into a <see cref="UrlServiceAnalysisDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="UrlServiceAnalysis"/> object to parse.</param>
    /// <returns>A new <see cref="UrlServiceAnalysisDto"/> object.</returns>
    public static UrlServiceAnalysisDto Parse(UrlServiceAnalysis source)
    {
        return new UrlServiceAnalysisDto(
            source.Id.Primary.Value,
            source.Id.Job,
            source.ServiceName,
            source.State.Status,
            source.State.Verdict,
            source.State.ThreatZone,
            source.ThreatScore,
            source.Error);
    }
}