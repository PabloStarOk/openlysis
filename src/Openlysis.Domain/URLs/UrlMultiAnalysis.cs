using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.Common.Models;
using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.Common.ServiceAnalyses.Mappings;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Domain.URLs;

/// <summary>
/// Represents a multi-analysis of a URL, containing multiple service analyses.
/// </summary>
public sealed class UrlMultiAnalysis : AggregateRoot<MultiAnalysisId>
{
    private readonly List<UrlServiceAnalysis> _serviceAnalyses = [];

    /// <summary>
    /// Gets the user ID associated with the analysis.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the analysis is private.
    /// </summary>
    public bool IsPrivate { get; }

    /// <summary>
    /// Gets the date when the analysis started.
    /// </summary>
    public DateTime StartedDate { get; }

    /// <summary>
    /// Gets the current status of the analysis.
    /// </summary>
    public AnalysisStatus Status { get; private set; } = AnalysisStatus.Queued;

    /// <summary>
    /// Gets the average verdict of the analysis.
    /// </summary>
    public Verdict AverageVerdict { get; private set; } = Verdict.Unknown;

    /// <summary>
    /// Gets the average threat zone of the analysis.
    /// </summary>
    public ThreatZone AverageThreatZone { get; private set; } = ThreatZone.Unknown;

    /// <summary>
    /// Gets the overall threat score of the <see cref="UrlServiceAnalysis"/>.
    /// </summary>
    public float? AverageThreatScore { get; private set; }

    /// <summary>
    /// Gets the URL being analyzed.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Gets the hash set of the URL content.
    /// </summary>
    public ContentHashSet UrlHashSet { get; }

    /// <summary>
    /// Gets the list of service analyses.
    /// </summary>
    public IReadOnlyList<UrlServiceAnalysis> ServiceAnalyses => _serviceAnalyses;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-analysis.</param>
    /// <param name="serviceAnalyses">The list of service analyses.</param>
    /// <param name="userId">The user ID associated with the analysis.</param>
    /// <param name="isPrivate">A value indicating whether the analysis is private.</param>
    /// <param name="startedDate">The date when the analysis started.</param>
    /// <param name="url">The URL being analyzed.</param>
    /// <param name="urlHashSet">The hash set of the URL content.</param>
    private UrlMultiAnalysis(
        MultiAnalysisId id,
        List<UrlServiceAnalysis> serviceAnalyses,
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        Uri url,
        ContentHashSet urlHashSet)
        : base(id)
    {
        _serviceAnalyses = serviceAnalyses;
        UserId = userId;
        IsPrivate = isPrivate;
        StartedDate = startedDate;
        Url = url;
        UrlHashSet = urlHashSet;
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
    /// <param name="maxServiceAnalysesAmount">The maximum number of service analyses allowed.</param>
    /// <param name="userId">The user ID associated with the analysis.</param>
    /// <param name="isPrivate">A value indicating whether the analysis is private.</param>
    /// <param name="startedDate">The date when the analysis started.</param>
    /// <param name="url">The URL being analyzed.</param>
    /// <param name="urlHashSet">The hash set of the URL content.</param>
    /// <returns>A new instance of the <see cref="UrlMultiAnalysis"/> class.</returns>
    public static UrlMultiAnalysis Create(
        int maxServiceAnalysesAmount,
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        Uri url,
        ContentHashSet urlHashSet)
    {
        List<UrlServiceAnalysis> serviceAnalyses = new (maxServiceAnalysesAmount);

        return new UrlMultiAnalysis(
            MultiAnalysisId.CreateUnique(),
            serviceAnalyses,
            userId,
            isPrivate,
            startedDate,
            url,
            urlHashSet);
    }

    /// <summary>
    /// Adds a new service file analysis to the collection.
    /// </summary>
    /// <param name="analysis">The service file analysis to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the multi-analysis already contains the same service analysis to be added.</exception>
    public void AddServiceAnalysis(UrlServiceAnalysis analysis)
    {
        if (_serviceAnalyses.Contains(analysis))
        {
            throw new InvalidOperationException("UrlMultiAnalysis already contains the given UrlServiceAnalysis.");
        }

        _serviceAnalyses.Add(analysis);
        UpdateAverageVerdict();
        UpdateAverageThreatZone();
        UpdateAverageThreatScore();
        UpdateStatus();
    }

    /// <summary>
    /// Updates an existing service file analysis in the collection.
    /// </summary>
    /// <param name="analysis">The service file analysis to update.</param>
    /// <exception cref="InvalidOperationException">Thrown when the multi-analysis doesn't contain the given service analysis to be updated.</exception>
    public void UpdateServiceAnalysis(UrlServiceAnalysis analysis)
    {
        if (!_serviceAnalyses.Contains(analysis))
        {
            throw new InvalidOperationException("UrlMultiAnalysis does not contain the given UrlServiceAnalysis.");
        }

        int analysisIndex = _serviceAnalyses.IndexOf(analysis);
        _serviceAnalyses[analysisIndex].UpdateVerdict(analysis.Verdict);
        _serviceAnalyses[analysisIndex].UpdateThreatScore(analysis.ThreatScore);
        _serviceAnalyses[analysisIndex].UpdateStatus(analysis.Status);
        UpdateAverageVerdict();
        UpdateAverageThreatZone();
        UpdateAverageThreatScore();
        UpdateStatus();
    }

    /// <summary>
    /// Updates the status of the file analysis based on the statuses of all service file analyses.
    /// </summary>
    private void UpdateStatus()
    {
        if (_serviceAnalyses.Count < 1)
        {
            return;
        }

        IEnumerable<UrlServiceAnalysis> analyses = _serviceAnalyses;

        // If all timeout, set as timeout
        if (analyses.All(a => a.Status is AnalysisStatus.Timeout))
        {
            Status = AnalysisStatus.Timeout;
            return;
        }

        // If all failed, set as failed
        if (analyses.All(a => a.Status is AnalysisStatus.Failed))
        {
            Status = AnalysisStatus.Failed;
            return;
        }

        // If is not queued nor in-progress and there's at least one completed, set as completed.
        if (analyses.All(a => a.Status is not AnalysisStatus.Queued and not AnalysisStatus.InProgress)
            && analyses.Any(a => a.Status is AnalysisStatus.Completed))
        {
            Status = AnalysisStatus.Completed;
            return;
        }

        // Queued or in progress according to most frequent or higher status.
        var statusCount = analyses.GroupBy(a => a.Status)
            .ToDictionary(g => g.Key, g => g.Count())
            .Where(g => g.Key
                is not AnalysisStatus.Completed
                and not AnalysisStatus.Timeout
                and not AnalysisStatus.Failed);

        Status = statusCount.OrderByDescending(s => s.Value)
            .ThenBy(s => s.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the average verdict based on all reports.
    /// </summary>
    private void UpdateAverageVerdict()
    {
        if (_serviceAnalyses.Count == 0)
        {
            AverageVerdict = Verdict.Unknown;
            return;
        }

        var verdictCounts = _serviceAnalyses
            .GroupBy(s => s.Verdict)
            .ToDictionary(g => g.Key, g => g.Count());

        AverageVerdict = verdictCounts
            .OrderByDescending(pair => pair.Value)
            .ThenByDescending(pair => pair.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the average threat zone based on all reports.
    /// </summary>
    private void UpdateAverageThreatZone()
    {
        AverageThreatZone = ThreatZoneMapping.Map[AverageVerdict];
    }

    /// <summary>
    /// Updates the average threat score based on all service analyses.
    /// </summary>
    private void UpdateAverageThreatScore()
    {
        if (!_serviceAnalyses.Any(s => s.ThreatScore is not null))
        {
            return;
        }

        AverageThreatScore = _serviceAnalyses
            .Where(s => s.ThreatScore is not null)
            .Select(s => s.ThreatScore)
            .Average();
    }
}