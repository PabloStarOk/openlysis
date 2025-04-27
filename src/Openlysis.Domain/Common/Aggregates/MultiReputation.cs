using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Aggregates;

/// <summary>
/// Represents a base aggregate for managing reputations from multiple services.
/// </summary>
/// <typeparam name="TServiceReputation">
/// The type of service reputation, which must inherit from <see cref="ServiceReputation"/>.
/// </typeparam>
public abstract class MultiReputation<TServiceReputation>
    : AggregateRoot<GlobalId>
    where TServiceReputation : ServiceReputation
{
    private readonly List<TServiceReputation> _servicesReputations = [];

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
    public IReadOnlyList<TServiceReputation> ServicesReputations => _servicesReputations;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiReputation{TServiceReputation}"/> class.
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
    /// Initializes a new instance of the <see cref="MultiReputation{TServiceReputation}"/> class for EF Core.
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
    /// Adds a new service reputation to the collection.
    /// </summary>
    /// <param name="serviceReputation">The service reputation to add. Must not already exist in the collection.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the given service reputation already exists in the collection.
    /// </exception>
    public void AddServiceReputation(TServiceReputation serviceReputation)
    {
        if (_servicesReputations.Contains(serviceReputation))
        {
            throw new ArgumentException(
                "Given ServiceReputation already exists in the collection.",
                nameof(serviceReputation));
        }

        _servicesReputations.Add(serviceReputation);
        UpdateVerdict();
        UpdateThreatZone();
    }

    /// <summary>
    /// Updates the final verdict based on the service reputations.
    /// </summary>
    private void UpdateVerdict()
    {
        Verdict[] servicesVerdicts = ServicesReputations
            .Select(s => s.Verdict)
            .ToArray();

        if (servicesVerdicts.Length is 0)
        {
            FinalVerdict = Verdict.Unknown;
            return;
        }

        if (servicesVerdicts.Contains(Verdict.Malicious))
        {
            FinalVerdict = Verdict.Malicious;
            return;
        }

        if (servicesVerdicts.Contains(Verdict.Suspicious))
        {
            FinalVerdict = Verdict.Suspicious;
            return;
        }

        if (servicesVerdicts.Contains(Verdict.Undetected))
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