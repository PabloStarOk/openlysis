using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Domain.Phones;

/// <summary>
/// Represents multiple reputations for a phone from different services.
/// </summary>
public class PhoneMultiReputation : MultiReputation<PhoneServiceReputation>
{
    private PhoneMultiReputation(
        GlobalId id,
        DateTime reputationEvaluationDate,
        Verdict averageVerdict,
        ThreatZone averageThreatZone)
        : base(id, reputationEvaluationDate, averageVerdict, averageThreatZone)
    {
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
    /// Creates a new instance of <see cref="PhoneMultiReputation"/>.
    /// </summary>
    /// <param name="reputationEvaluationDate">The date when the reputation was evaluated.</param>
    /// <returns>A new instance of <see cref="PhoneMultiReputation"/>.</returns>
    public static PhoneMultiReputation Create(
        DateTime reputationEvaluationDate)
    {
        GlobalId globalId = GlobalId.CreateUnique();
        return new PhoneMultiReputation(
            globalId,
            reputationEvaluationDate,
            Verdict.Unknown,
            ThreatZone.Unknown);
    }
}