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
    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneMultiReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the reputation.</param>
    /// <param name="reputationEvaluationDate">The date when the reputation was evaluated.</param>
    /// <param name="finalVerdict">The final verdict of the reputation.</param>
    /// <param name="finalThreatZone">The final threat zone of the reputation.</param>
    private PhoneMultiReputation(
        GlobalId id,
        DateTime reputationEvaluationDate,
        Verdict finalVerdict,
        ThreatZone finalThreatZone)
        : base(id, reputationEvaluationDate, finalVerdict, finalThreatZone)
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