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
    /// Gets the overall threat score of the <see cref="UrlServiceAnalysis"/>.
    /// </summary>
    public float? AverageThreatScore { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-analysis.</param>
    /// <param name="userId">The user ID associated with the analysis.</param>
    /// <param name="isPrivate">A value indicating whether the analysis is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="averageVerdict">The average verdict of the analysis.</param>
    /// <param name="averageThreatZone">The average threat zone of the analysis.</param>
    /// <param name="urlHashSet">The hash set of the URL content.</param>
    /// <param name="averageThreatScore">The average threat score of the analysis.</param>
    /// <param name="url">The URL being analyzed.</param>
    private UrlMultiAnalysis(
        GlobalId id,
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        AnalysisStatus status,
        Verdict averageVerdict,
        ThreatZone averageThreatZone,
        ContentHashSet urlHashSet,
        Uri url,
        float? averageThreatScore)
        : base(id, userId, isPrivate, startedDate, status, averageVerdict, averageThreatZone, urlHashSet)
    {
        Url = url;
        AverageThreatScore = averageThreatScore;
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
            url,
            null);
    }

    /// <inheritdoc/>
    protected override void HandleServiceAnalysisUpdate(int index, UrlServiceAnalysis analysis)
    {
        InternalServiceAnalyses[index].UpdateVerdict(analysis.Verdict);
        InternalServiceAnalyses[index].UpdateThreatScore(analysis.ThreatScore);
        InternalServiceAnalyses[index].UpdateStatus(analysis.Status);
    }

    /// <inheritdoc/>
    protected override void OnUpdateInformation()
    {
        UpdateAverageThreatScore();
    }

    /// <inheritdoc/>
    protected override void HandleAverageVerdictUpdate()
    {
        if (InternalServiceAnalyses.Count == 0)
        {
            AverageVerdict = Verdict.Unknown;
            return;
        }

        var verdictCounts = InternalServiceAnalyses
            .GroupBy(s => s.Verdict)
            .ToDictionary(g => g.Key, g => g.Count());

        AverageVerdict = verdictCounts
            .OrderByDescending(pair => pair.Value)
            .ThenByDescending(pair => pair.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the average threat score based on all service analyses.
    /// </summary>
    private void UpdateAverageThreatScore()
    {
        if (!InternalServiceAnalyses.Any(s => s.ThreatScore is not null))
        {
            return;
        }

        AverageThreatScore = InternalServiceAnalyses
            .Where(s => s.ThreatScore is not null)
            .Select(s => s.ThreatScore)
            .Average();
    }
}