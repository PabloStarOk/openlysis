using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
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
    /// <param name="state">The initial state of the analysis, including its status, verdict, and threat zone.</param>
    /// <param name="urlHashValues">The hash set of the URL content.</param>
    /// <param name="url">The URL being analyzed.</param>
    private UrlMultiAnalysis(
        GlobalId id,
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        AnalysisState state,
        HashValues urlHashValues,
        Uri url)
        : base(
            id,
            userId,
            isPrivate,
            startedDate,
            state,
            urlHashValues)
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
    /// <param name="urlHashValues">The hash set of the URL content.</param>
    /// <returns>A new instance of the <see cref="UrlMultiAnalysis"/> class.</returns>
    public static UrlMultiAnalysis Create(
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        Uri url,
        HashValues urlHashValues)
    {
        return new UrlMultiAnalysis(
            GlobalId.CreateUnique(),
            userId,
            isPrivate,
            startedDate,
            AnalysisState.Initial(),
            urlHashValues,
            url);
    }

    /// <inheritdoc/>
    protected override void HandleServiceAnalysisUpdate(
        UrlServiceAnalysis existingAnalysis,
        UrlServiceAnalysis updatedAnalysis)
    {
        existingAnalysis.UpdateVerdict(updatedAnalysis.State.Verdict);
        existingAnalysis.UpdateThreatScore(updatedAnalysis.ThreatScore);
        existingAnalysis.UpdateStatus(updatedAnalysis.State.Status);
    }

    /// <inheritdoc/>
    protected override void HandleAverageThreatScoreUpdate()
    {
        if (ServiceAnalyses.All(s => s.ThreatScore.NormalizedValue is null))
        {
            return;
        }

        double? average = ServiceAnalyses
            .Where(s => s.ThreatScore.NormalizedValue is not null)
            .Select(s => s.ThreatScore.NormalizedValue)
            .Average();
        if (!average.HasValue)
        {
            return;
        }

        AverageThreatScore = (int?)Math.Round(average.Value);
    }
}