using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.URLs;

namespace Openlysis.API.Endpoints.URLs.Common;

/// <summary>
/// Data transfer object for <see cref="UrlMultiAnalysis"/>.
/// </summary>
/// <param name="Id">The unique identifier of the analysis.</param>
/// <param name="IsPrivate">Indicates if the analysis is private.</param>
/// <param name="StartedDate">The date when the analysis started.</param>
/// <param name="Status">The current status of the analysis.</param>
/// <param name="FinalVerdict">The final verdict of the analysis.</param>
/// <param name="FinalThreatZone">The final threat zone of the analysis.</param>
/// <param name="AverageThreatScore">The average threat score of the analysis.</param>
/// <param name="Url">The URL being analyzed.</param>
/// <param name="UrlHashSet">The set of content hashes for the URL.</param>
/// <param name="ServiceAnalyses">The analyses from different services.</param>
public record UrlMultiAnalysisDto(
    string Id,
    bool IsPrivate,
    DateTime StartedDate,
    AnalysisStatus Status,
    Verdict FinalVerdict,
    ThreatZone FinalThreatZone,
    float? AverageThreatScore,
    Uri Url,
    ContentHashSet UrlHashSet,
    UrlServiceAnalysisDto[] ServiceAnalyses)
{
    /// <summary>
    /// Parses a <see cref="UrlMultiAnalysis"/> object into a <see cref="UrlMultiAnalysisDto"/>.
    /// </summary>
    /// <param name="source">The <see cref="UrlMultiAnalysis"/> object to parse.</param>
    /// <returns>A <see cref="UrlMultiAnalysisDto"/> object.</returns>
    public static UrlMultiAnalysisDto Parse(UrlMultiAnalysis source)
    {
        UrlServiceAnalysisDto[] serviceAnalyses = source.ServiceAnalyses
            .Select(UrlServiceAnalysisDto.Parse)
            .ToArray();

        return new UrlMultiAnalysisDto(
            source.Id.Value.ToString(),
            source.IsPrivate,
            source.StartedDate,
            source.Status,
            source.FinalVerdict,
            source.FinalThreatZone,
            source.AverageThreatScore,
            source.Url,
            source.DataHashSet,
            serviceAnalyses);
    }
}