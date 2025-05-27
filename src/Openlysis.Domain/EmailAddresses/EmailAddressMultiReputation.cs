using System.Net.Mail;

using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses.Entities;

namespace Openlysis.Domain.EmailAddresses;

/// <summary>
/// An aggregate that contains multiple reputations for an email address.
/// </summary>
public class EmailAddressMultiReputation : MultiReputation<EmailAddressReputation>
{
    /// <summary>
    /// Gets the email address associated with the reputation.
    /// </summary>
    public MailAddress EmailAddress { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressMultiReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the multi-reputation.</param>
    /// <param name="evaluationReputationDate">The date when the reputation was evaluated.</param>
    /// <param name="finalVerdict">The final verdict of the multi-reputation.</param>
    /// <param name="finalThreatZone">The final threat zone of the multi-reputation.</param>
    /// <param name="emailAddress">The email address associated with the multi-reputation.</param>
    private EmailAddressMultiReputation(
        GlobalId id,
        DateTime evaluationReputationDate,
        Verdict finalVerdict,
        ThreatZone finalThreatZone,
        MailAddress emailAddress)
        : base(id, evaluationReputationDate, finalVerdict, finalThreatZone)
    {
        EmailAddress = emailAddress;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private EmailAddressMultiReputation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="EmailAddressMultiReputation"/>.
    /// </summary>
    /// <param name="reputationEvaluationDate">The date when the reputation was evaluated.</param>
    /// <param name="emailAddress">The email address associated with the multi-reputation.</param>
    /// <returns>A new instance of <see cref="EmailAddressMultiReputation"/>.</returns>
    public static EmailAddressMultiReputation Create(
        DateTime reputationEvaluationDate,
        MailAddress emailAddress)
    {
        GlobalId globalId = GlobalId.CreateUnique();
        return new EmailAddressMultiReputation(
            globalId,
            reputationEvaluationDate,
            Verdict.Unknown,
            ThreatZone.Unknown,
            emailAddress);
    }
}