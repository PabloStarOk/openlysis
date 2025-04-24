using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Domain.URLs;

/// <summary>
/// Represents a multi-analysis of a URL, containing multiple service analyses.
/// </summary>
public sealed class UrlMultiAnalysis : MultiAnalysis<UrlServiceAnalysis>
{
    /// <summary>
    /// Gets the URL being analyzed.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-analysis.</param>
    /// <param name="userId">The user ID associated with the analysis.</param>
    /// <param name="isPrivate">A value indicating whether the analysis is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="finalVerdict">The final verdict of the analysis.</param>
    /// <param name="finalThreatZone">The final threat zone of the analysis.</param>
    /// <param name="urlHashSet">The hash set of the URL content.</param>
    /// <param name="url">The URL being analyzed.</param>
    private UrlMultiAnalysis(
        GlobalId id,
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        AnalysisStatus status,
        Verdict finalVerdict,
        ThreatZone finalThreatZone,
        ContentHashSet urlHashSet,
        Uri url)
        : base(
            id,
            userId,
            isPrivate,
            startedDate,
            status,
            finalVerdict,
            finalThreatZone,
            urlHashSet)
    {
        Url = url;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private UrlMultiAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="UrlMultiAnalysis"/> class.
    /// </summary>
    /// <param name="userId">The user ID associated with the analysis.</param>
    /// <param name="isPrivate">A value indicating whether the analysis is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="url">The URL being analyzed.</param>
    /// <param name="urlHashSet">The hash set of the URL content.</param>
    /// <returns>A new instance of the <see cref="UrlMultiAnalysis"/> class.</returns>
    public static UrlMultiAnalysis Create(
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        Uri url,
        ContentHashSet urlHashSet)
    {
        return new UrlMultiAnalysis(
            GlobalId.CreateUnique(),
            userId,
            isPrivate,
            startedDate,
            AnalysisStatus.Queued,
            Verdict.Unknown,
            ThreatZone.Unknown,
            urlHashSet,
            url);
    }

    /// <inheritdoc/>
    protected override void HandleServiceAnalysisUpdate(
        UrlServiceAnalysis existingAnalysis,
        UrlServiceAnalysis updatedAnalysis)
    {
        existingAnalysis.UpdateVerdict(updatedAnalysis.Verdict);
        existingAnalysis.UpdateThreatScore(updatedAnalysis.ThreatScore);
        existingAnalysis.UpdateStatus(updatedAnalysis.Status);
    }

    /// <inheritdoc/>
    protected override Verdict[] GetServiceAnalysesVerdicts()
    {
        return ServiceAnalyses
            .Select(r => r.Verdict)
            .ToArray();
    }

    /// <inheritdoc/>
    protected override void HandleAverageThreatScoreUpdate()
    {
        if (ServiceAnalyses.All(s => s.ThreatScore is null))
        {
            return;
        }

        AverageThreatScore = ServiceAnalyses
            .Where(s => s.ThreatScore is not null)
            .Select(s => s.ThreatScore)
            .Average();
    }
}