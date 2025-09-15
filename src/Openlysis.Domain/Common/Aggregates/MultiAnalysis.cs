using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Aggregates;

/// <summary>
/// A base aggregate that contains multiple analyses performed by external services.
/// </summary>
/// <typeparam name="TAnalysis">
/// The type of  analysis associated with the multi-analysis.
/// Must inherit from <see cref="Analysis"/>.
/// </typeparam>
public abstract class MultiAnalysis<TAnalysis>
    : AggregateRoot<GlobalId>
    where TAnalysis : Analysis
{
    private readonly List<TAnalysis> _analyses = [];

    /// <summary>
    /// Gets the identifier of the user associated with the analysis.
    /// </summary>
    public GlobalId UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the analysis is private.
    /// </summary>
    public bool IsPrivate { get; }

    /// <summary>
    /// Gets the date and time when the analysis started.
    /// </summary>
    public DateTimeOffset StartedDate { get; }

    /// <summary>
    /// Gets the current state of the analysis.
    /// </summary>
    public AnalysisState State { get; private set; }

    /// <summary>
    /// Gets or sets the average threat score of the analysis.
    /// </summary>
    public int? AverageThreatScore { get; protected set; }

    /// <summary>
    /// Gets the set of data hashes associated with the analysis.
    /// </summary>
    public HashValues DataHashValues { get; }

    /// <summary>
    /// Gets analyses of the multi-analysis.
    /// </summary>
    public IReadOnlyList<TAnalysis> Analyses => _analyses;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalysis{TAnalysis}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-analysis.</param>
    /// <param name="userId">The identifier of the user associated with the analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="state">The initial state of the analysis, including its status, verdict, and threat zone.</param>
    /// <param name="dataHashValues">The set of data hashes associated with the analysis.</param>
    protected MultiAnalysis(
        GlobalId id,
        GlobalId userId,
        bool isPrivate,
        DateTimeOffset startedDate,
        AnalysisState state,
        HashValues dataHashValues)
        : base(id)
    {
        UserId = userId;
        IsPrivate = isPrivate;
        StartedDate = startedDate;
        State = state;
        DataHashValues = dataHashValues;
    }

    // For EF Core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalysis{TAnalysis}"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    protected MultiAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Adds new analyses to the multi-analysis or updates existing ones.
    /// If an analysis already exists, it is updated; otherwise, it is added.
    /// Updates the overall information after processing.
    /// </summary>
    /// <param name="analyses">The analyses to add or update.</param>
    public void AddOrUpdateAnalyses(params TAnalysis[] analyses)
    {
        foreach (TAnalysis analysis in analyses)
        {
            if (_analyses.Contains(analysis))
            {
                UpdateAnalysis(analysis);
                continue;
            }

            AddAnalysis(analysis);
        }

        UpdateInformation();
    }

    /// <summary>
    /// Sets the state of the multi-analysis and its updatable analyses to the specified status.
    /// Updates the status of all analyses that can be updated to the given <paramref name="status"/>.
    /// </summary>
    /// <param name="status">The <see cref="AnalysisStatus"/> to set for the multi-analysis and its analyses.</param>
    /// <remarks>
    /// If the multi-analysis or all contained analyses cannot be updated, the method returns without changes.
    /// </remarks>
    public void SetAs(AnalysisStatus status)
    {
        if (!State.CanBeUpdated)
        {
            return;
        }

        if (_analyses.Count is 0)
        {
            State = State.WithStatus(status);
            return;
        }

        if (_analyses.All(a => !a.State.CanBeUpdated))
        {
            return;
        }

        foreach (var analysis in _analyses.Where(a => a.State.CanBeUpdated))
        {
            analysis.UpdateStatus(status);
        }

        UpdateStatus();
    }

    /// <summary>
    /// Invoked when an existing analysis in the multi-analysis is updated.
    /// </summary>
    /// <param name="existingAnalysis">The current analysis instance that exists in the multi-analysis.</param>
    /// <param name="updatedAnalysis">The updated analysis instance that will replace the existing one.</param>
    protected abstract void HandleAnalysisUpdate(
        TAnalysis existingAnalysis,
        TAnalysis updatedAnalysis);

    /// <summary>
    /// Updates the average threat score of the multi-analysis based on the associated analyses.
    /// </summary>
    protected abstract void HandleAverageThreatScoreUpdate();

    /// <summary>
    /// Adds a new analysis to the multi-analysis.
    /// </summary>
    /// <param name="analysis">The analysis to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided analysis is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the analysis already exists in the collection.</exception>
    private void AddAnalysis(TAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        if (_analyses.Contains(analysis))
        {
            throw new InvalidOperationException("Multi-analysis already contains the given analysis.");
        }

        _analyses.Add(analysis);
    }

    /// <summary>
    /// Updates an existing analysis in the collection.
    /// </summary>
    /// <param name="analysis">The analysis to update.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the analysis does not exist in the collection.
    /// </exception>
    private void UpdateAnalysis(TAnalysis analysis)
    {
        if (!_analyses.Contains(analysis))
        {
            throw new InvalidOperationException("Analysis does not exist in the multi-analysis.");
        }

        TAnalysis existingAnalysis = _analyses.Single(a => a == analysis);
        if (existingAnalysis.State is not
            {
                Status: AnalysisStatus.Queued or AnalysisStatus.InProgress,
            })
        {
            return;
        }

        HandleAnalysisUpdate(existingAnalysis, analysis);
    }

    /// <summary>
    /// Updates the final verdict of the multi-analysis based on the associated analyses.
    /// </summary>
    private void UpdateFinalVerdict()
    {
        Verdict[] analysesVerdicts = _analyses
            .Select(s => s.State.Verdict)
            .ToArray();

        if (analysesVerdicts.Length is 0)
        {
            State = State.WithVerdict(Verdict.Unknown);
            return;
        }

        if (analysesVerdicts.Contains(Verdict.Malicious))
        {
            State = State.WithVerdict(Verdict.Malicious);
            return;
        }

        if (analysesVerdicts.Contains(Verdict.Suspicious))
        {
            State = State.WithVerdict(Verdict.Suspicious);
            return;
        }

        if (analysesVerdicts.Contains(Verdict.Undetected))
        {
            State = State.WithVerdict(Verdict.Undetected);
        }
    }

    /// <summary>
    /// Updates the status of the multi-analysis based on the statuses of the associated analyses.
    /// </summary>
    /// <remarks>
    /// The method evaluates the statuses of all analyses in the multi-analysis and determines the most appropriate
    /// overall status for the multi-analysis. It handles scenarios such as all analyses timing out, failing, or completing,
    /// and prioritizes the most frequent or highest status for queued or in-progress analyses.
    /// </remarks>
    private void UpdateStatus()
    {
        if (_analyses.Count < 1)
        {
            return;
        }

        IEnumerable<TAnalysis> analyses = _analyses;

        // If all timeout, set as timeout
        if (analyses.All(a => a.State.Status is AnalysisStatus.Timeout))
        {
            State = State.WithStatus(AnalysisStatus.Timeout);
            return;
        }

        // If all failed, set as failed
        if (analyses.All(a => a.State.Status is AnalysisStatus.Failed))
        {
            State = State.WithStatus(AnalysisStatus.Failed);
            return;
        }

        // If is not queued nor in-progress and there's at least one completed, set as completed.
        if (analyses.All(a => a.State.Status is not AnalysisStatus.Queued and not AnalysisStatus.InProgress)
            && analyses.Any(a => a.State.Status is AnalysisStatus.Completed))
        {
            State = State.WithStatus(AnalysisStatus.Completed);
            return;
        }

        // The most frequent and lower status.
        var statusCount = analyses.GroupBy(a => a.State.Status)
            .ToDictionary(g => g.Key, g => g.Count())
            .Where(g => g.Key is not AnalysisStatus.Completed);

        AnalysisStatus mostFrequentLowerStatus = statusCount.OrderByDescending(s => s.Value)
            .ThenBy(s => s.Key)
            .First().Key;

        State = State.WithStatus(mostFrequentLowerStatus);
    }

    /// <summary>
    /// Updates the information of the multi-analysis, including the final verdict,
    /// final threat zone, and overall status.
    /// </summary>
    private void UpdateInformation()
    {
        UpdateFinalVerdict();
        HandleAverageThreatScoreUpdate();
        UpdateStatus();
    }
}