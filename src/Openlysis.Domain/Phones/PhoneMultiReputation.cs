using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones.Entities;

namespace Openlysis.Domain.Phones;

/// <summary>
/// An aggregate that contains multiple reputations for a phone number.
/// </summary>
public class PhoneMultiReputation : MultiReputation<PhoneReputation>
{
    /// <summary>
    /// Gets the phone number associated with the multi-reputation.
    /// </summary>
    public string PhoneNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneMultiReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-reputation.</param>
    /// <param name="reputationEvaluationDate">The date when the multi-reputation was evaluated.</param>
    /// <param name="finalVerdict">The final verdict of the multi-reputation.</param>
    /// <param name="finalThreatZone">The final threat zone of the multi-reputation.</param>
    /// <param name="phoneNumber">The phone number associated with the multi-reputation.</param>
    private PhoneMultiReputation(
        GlobalId id,
        DateTimeOffset reputationEvaluationDate,
        Verdict finalVerdict,
        ThreatZone finalThreatZone,
        string phoneNumber)
        : base(id, reputationEvaluationDate, finalVerdict, finalThreatZone)
    {
        PhoneNumber = phoneNumber;
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
    /// <param name="reputationEvaluationDate">The date when the multi-reputation was evaluated.</param>
    /// <param name="phoneNumber">The phone number associated with the multi-reputation.</param>
    /// <returns>A new instance of <see cref="PhoneMultiReputation"/>.</returns>
    public static PhoneMultiReputation Create(
        DateTimeOffset reputationEvaluationDate,
        string phoneNumber)
    {
        GlobalId globalId = GlobalId.CreateUnique();
        return new PhoneMultiReputation(
            globalId,
            reputationEvaluationDate,
            Verdict.Unknown,
            ThreatZone.Unknown,
            phoneNumber);
    }
}