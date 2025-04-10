using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Domain.Phones;

/// <summary>
/// Represents multiple reputations for a phone from different services.
/// </summary>
public class PhoneMultiReputation : AggregateRoot<Id>
{
    private readonly List<PhoneServiceReputation> _servicesReputations = [];

    /// <summary>
    /// Gets the date of the assessment.
    /// </summary>
    public DateTime AssessmentDate { get; }

    /// <summary>
    /// Gets the average verdict of the multi-reputation.
    /// </summary>
    public Verdict AverageVerdict { get; private set; }

    /// <summary>
    /// Gets the average threat zone of the multi-reputation.
    /// </summary>
    public ThreatZone AverageThreatZone { get; private set; }

    /// <summary>
    /// Gets a list of reputations of different services.
    /// </summary>
    public IReadOnlyList<PhoneServiceReputation> ServicesReputations => _servicesReputations;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneMultiReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-reputation.</param>
    /// <param name="assessmentDate">The date of the assessment.</param>
    /// <param name="averageVerdict">The average verdict of the reputation.</param>
    /// <param name="averageThreatZone">The average threat zone of the reputation.</param>
    private PhoneMultiReputation(
        Id id,
        DateTime assessmentDate,
        Verdict averageVerdict,
        ThreatZone averageThreatZone)
        : base(id)
    {
        AssessmentDate = assessmentDate;
        AverageVerdict = averageVerdict;
        AverageThreatZone = averageThreatZone;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private PhoneMultiReputation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="PhoneMultiReputation"/> with the specified assessment date.
    /// </summary>
    /// <param name="assessmentDate">The date when the reputation was validated.</param>
    /// <returns>A new instance of <see cref="PhoneMultiReputation"/>.</returns>
    public static PhoneMultiReputation Create(
        DateTime assessmentDate)
    {
        Id id = Id.CreateUnique();
        return new PhoneMultiReputation(
            id,
            assessmentDate,
            Verdict.Unknown,
            ThreatZone.Unknown);
    }

    /// <summary>
    /// Adds a new reputation given by a service to the collection.
    /// </summary>
    /// <param name="serviceReputation">The reputation to add.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the reputation entity already exists in the collection.
    /// </exception>
    public void AddServiceReputation(PhoneServiceReputation serviceReputation)
    {
        if (_servicesReputations.Contains(serviceReputation))
        {
            throw new ArgumentException(
                "Given PhoneServiceReputation already exists in the collection.",
                nameof(serviceReputation));
        }

        _servicesReputations.Add(serviceReputation);
        UpdateVerdict();
        UpdateThreatZone();
    }

    /// <summary>
    /// Updates the average verdict based on the service reputations.
    /// </summary>
    private void UpdateVerdict()
    {
        if (_servicesReputations.Count is 0)
        {
            AverageVerdict = Verdict.Unknown;
            return;
        }

        var verdictCounts = _servicesReputations
            .GroupBy(s => s.Verdict)
            .ToDictionary(g => g.Key, g => g.Count());

        AverageVerdict = verdictCounts
            .OrderByDescending(pair => pair.Value)
            .ThenByDescending(pair => pair.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the average threat zone based on the average verdict.
    /// </summary>
    private void UpdateThreatZone()
    {
        AverageThreatZone = ThreatZoneMapping.Map[AverageVerdict];
    }
}