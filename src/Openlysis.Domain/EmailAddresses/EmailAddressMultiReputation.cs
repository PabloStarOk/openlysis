using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses.Entities;

namespace Openlysis.Domain.EmailAddresses;

/// <summary>
/// Represents the multi-reputation of an email address, which aggregates reputations
/// from various email address service providers.
/// </summary>
public class EmailAddressMultiReputation : MultiReputation<EmailAddressServiceReputation>
{
    /// <summary>
    /// Gets the email address associated with the reputation.
    /// </summary>
    public string EmailAddress { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressMultiReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the reputation.</param>
    /// <param name="evaluationReputationDate">The date when the reputation was evaluated.</param>
    /// <param name="averageVerdict">The average verdict of the reputation.</param>
    /// <param name="averageThreatZone">The average threat zone of the reputation.</param>
    /// <param name="emailAddress">The email address associated with the reputation.</param>
    private EmailAddressMultiReputation(
        GlobalId id,
        DateTime evaluationReputationDate,
        Verdict averageVerdict,
        ThreatZone averageThreatZone,
        string emailAddress)
        : base(id, evaluationReputationDate, averageVerdict, averageThreatZone)
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
    /// <param name="emailAddress">The email address associated with the reputation.</param>
    /// <returns>A new instance of <see cref="EmailAddressMultiReputation"/>.</returns>
    public static EmailAddressMultiReputation Create(
        DateTime reputationEvaluationDate,
        string emailAddress)
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