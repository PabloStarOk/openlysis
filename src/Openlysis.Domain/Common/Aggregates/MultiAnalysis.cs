using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Domain.Common.Aggregates;

/// <summary>
/// Defines a base aggregate for multiple analyses from external services.
/// </summary>
/// <typeparam name="TServiceAnalysis">
/// The type of service analysis associated with the multi-analysis.
/// Must inherit from <see cref="ServiceAnalysis"/>.
/// </typeparam>
public abstract class MultiAnalysis<TServiceAnalysis>
    : AggregateRoot<GlobalId>
    where TServiceAnalysis : ServiceAnalysis
{
    private readonly List<TServiceAnalysis> _serviceAnalyses = [];

    /// <summary>
    /// Gets the identifier of the user associated with the analysis.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the analysis is private.
    /// </summary>
    public bool IsPrivate { get; }

    /// <summary>
    /// Gets the date and time when the analysis started.
    /// </summary>
    public DateTime StartedDate { get; }

    /// <summary>
    /// Gets the current state of the analysis.
    /// </summary>
    public AnalysisState State { get; private set; }

    /// <summary>
    /// Gets or sets the average threat score of the analysis.
    /// </summary>
    public float? AverageThreatScore { get; protected set; }

    /// <summary>
    /// Gets the set of data hashes associated with the analysis.
    /// </summary>
    public HashValues DataHashValues { get; }

    /// <summary>
    /// Gets the list of service analyses associated with the analysis.
    /// </summary>
    public IReadOnlyList<TServiceAnalysis> ServiceAnalyses => _serviceAnalyses;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalysis{TServiceAnalysis}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-analysis.</param>
    /// <param name="userId">The identifier of the user associated with the analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="state">The initial state of the analysis, including its status, verdict, and threat zone.</param>
    /// <param name="dataHashValues">The set of data hashes associated with the analysis.</param>
    protected MultiAnalysis(
        GlobalId id,
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
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
    /// Initializes a new instance of the <see cref="MultiAnalysis{TServiceAnalysis}"/> class for EF Core.
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
    /// Adds a new service analysis to the collection.
    /// </summary>
    /// <param name="analysis">The service analysis to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided analysis is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the analysis already exists in the collection.</exception>
    public void AddServiceAnalysis(TServiceAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        if (_serviceAnalyses.Contains(analysis))
        {
            throw new InvalidOperationException("Service analysis already contains the given UrlServiceAnalysis.");
        }

        _serviceAnalyses.Add(analysis);
        UpdateInformation();
    }

    /// <summary>
    /// Updates an existing service analysis in the collection.
    /// </summary>
    /// <param name="analysis">The service analysis to update.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the service analysis does not exist in the collection.
    /// </exception>
    public void UpdateServiceAnalysis(TServiceAnalysis analysis)
    {
        if (!_serviceAnalyses.Contains(analysis))
        {
            throw new InvalidOperationException("Service analysis does not exist in the collection.");
        }

        TServiceAnalysis existingAnalysis = _serviceAnalyses.Single(a => a == analysis);
        if (existingAnalysis.State is not
            {
                Status: AnalysisStatus.Queued or AnalysisStatus.InProgress
            })
        {
            return;
        }

        HandleServiceAnalysisUpdate(existingAnalysis, analysis);
        UpdateInformation();
    }

    /// <summary>
    /// Invoked when an existing service analysis in the collection is updated.
    /// </summary>
    /// <param name="existingAnalysis">The current service analysis instance that exists in the collection.</param>
    /// <param name="updatedAnalysis">The updated service analysis instance that will replace the existing one.</param>
    protected abstract void HandleServiceAnalysisUpdate(
        TServiceAnalysis existingAnalysis,
        TServiceAnalysis updatedAnalysis);

    /// <summary>
    /// Updates the average threat score of the analysis based on the associated service analyses.
    /// </summary>
    protected abstract void HandleAverageThreatScoreUpdate();

    /// <summary>
    /// Updates the final verdict of the multi-analysis based on the associated service analyses.
    /// </summary>
    private void UpdateFinalVerdict()
    {
        Verdict[] servicesVerdicts = _serviceAnalyses
            .Select(s => s.State.Verdict)
            .ToArray();

        if (servicesVerdicts.Length is 0)
        {
            State = State.WithVerdict(Verdict.Unknown);
            return;
        }

        if (servicesVerdicts.Contains(Verdict.Malicious))
        {
            State = State.WithVerdict(Verdict.Malicious);
            return;
        }

        if (servicesVerdicts.Contains(Verdict.Suspicious))
        {
            State = State.WithVerdict(Verdict.Suspicious);
            return;
        }

        if (servicesVerdicts.Contains(Verdict.Undetected))
        {
            State = State.WithVerdict(Verdict.Undetected);
        }
    }

    /// <summary>
    /// Updates the overall status of the analysis based on the statuses of the associated service analyses.
    /// </summary>
    /// <remarks>
    /// The method evaluates the statuses of all service analyses in the collection and determines the most appropriate
    /// overall status for the analysis. It handles scenarios such as all analyses timing out, failing, or completing,
    /// and prioritizes the most frequent or highest status for queued or in-progress analyses.
    /// </remarks>
    private void UpdateStatus()
    {
        if (_serviceAnalyses.Count < 1)
        {
            return;
        }

        IEnumerable<TServiceAnalysis> analyses = _serviceAnalyses;

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