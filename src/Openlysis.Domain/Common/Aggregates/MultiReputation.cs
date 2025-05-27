using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Aggregates;

/// <summary>
/// A base aggregate that contains multiple reputations returned by external services.
/// </summary>
/// <typeparam name="TReputation">
/// The type of reputation, which must inherit from <see cref="Reputation"/>.
/// </typeparam>
public abstract class MultiReputation<TReputation>
    : AggregateRoot<GlobalId>
    where TReputation : Reputation
{
    private readonly List<TReputation> _reputations = [];

    /// <summary>
    /// Gets the date when the reputation evaluation was performed.
    /// </summary>
    public DateTime ReputationEvaluationDate { get; }

    /// <summary>
    /// Gets the final verdict of the multi-reputation.
    /// </summary>
    public Verdict FinalVerdict { get; private set; }

    /// <summary>
    /// Gets the final threat zone of the multi-reputation.
    /// </summary>
    public ThreatZone FinalThreatZone { get; private set; }

    /// <summary>
    /// Gets a list of reputations of different services.
    /// </summary>
    public IReadOnlyList<TReputation> Reputations => _reputations;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiReputation{TReputation}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate.</param>
    /// <param name="reputationEvaluationDate">The date when the reputation evaluation was performed.</param>
    /// <param name="finalVerdict">The initial final verdict of the multi-reputation.</param>
    /// <param name="finalThreatZone">The initial final threat zone of the multi-reputation.</param>
    protected MultiReputation(
        GlobalId id,
        DateTime reputationEvaluationDate,
        Verdict finalVerdict,
        ThreatZone finalThreatZone)
        : base(id)
    {
        ReputationEvaluationDate = reputationEvaluationDate;
        FinalVerdict = finalVerdict;
        FinalThreatZone = finalThreatZone;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiReputation{TReputation}"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor must not be used in the application code.
    /// </remarks>
    protected MultiReputation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Adds a new reputation to the multi-reputation.
    /// </summary>
    /// <param name="reputation">The reputation to add.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the given reputation already exists in the collection.
    /// </exception>
    public void AddReputation(TReputation reputation)
    {
        if (_reputations.Contains(reputation))
        {
            throw new ArgumentException(
                "Given reputation already exists in the collection.",
                nameof(reputation));
        }

        _reputations.Add(reputation);
        UpdateVerdict();
        UpdateThreatZone();
    }

    /// <summary>
    /// Updates the final verdict based on the reputations.
    /// </summary>
    private void UpdateVerdict()
    {
        Verdict[] reputationsVerdicts = Reputations
            .Select(s => s.Verdict)
            .ToArray();

        if (reputationsVerdicts.Length is 0)
        {
            FinalVerdict = Verdict.Unknown;
            return;
        }

        if (reputationsVerdicts.Contains(Verdict.Malicious))
        {
            FinalVerdict = Verdict.Malicious;
            return;
        }

        if (reputationsVerdicts.Contains(Verdict.Suspicious))
        {
            FinalVerdict = Verdict.Suspicious;
            return;
        }

        if (reputationsVerdicts.Contains(Verdict.Undetected))
        {
            FinalVerdict = Verdict.Undetected;
        }
    }

    /// <summary>
    /// Updates the final threat zone based on the final verdict.
    /// </summary>
    private void UpdateThreatZone()
    {
        FinalThreatZone = ThreatZoneMapping.Map[FinalVerdict];
    }
}