using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.EmailAddresses;

namespace Openlysis.API.Endpoints.EmailAddresses.GetReputation;

/// <summary>
/// Data transfer object for <see cref="EmailAddressMultiReputation"/>.
/// </summary>
/// <param name="Id">The unique identifier of the multi-reputation.</param>
/// <param name="EvaluationDate">The date when the reputation was evaluated.</param>
/// <param name="FinalVerdict">The final verdict assigned to the email address based on multiple reputations.</param>
/// <param name="FinalThreatZone">The final threat zone classification for the email address.</param>
/// <param name="EmailAddress">The email address associated with the multi-reputation.</param>
/// <param name="Reputations">A collection of <see cref="EmailAddressReputationDto"/>.</param>
public record EmailAddressMultiReputationDto(
    string Id,
    DateTimeOffset EvaluationDate,
    Verdict FinalVerdict,
    ThreatZone FinalThreatZone,
    string EmailAddress,
    IEnumerable<EmailAddressReputationDto> Reputations)
{
    /// <summary>
    /// Converts an <see cref="EmailAddressMultiReputation"/> instance to an <see cref="EmailAddressMultiReputationDto"/>.
    /// </summary>
    /// <param name="source">The source object containing email address reputation details.</param>
    /// <returns>An instance of <see cref="EmailAddressMultiReputationDto"/> representing the converted data.</returns>
    public static EmailAddressMultiReputationDto Parse(
        EmailAddressMultiReputation source)
    {
        return new EmailAddressMultiReputationDto(
            source.Id.ToString(),
            source.ReputationEvaluationDate,
            source.FinalVerdict,
            source.FinalThreatZone,
            source.EmailAddress.Address,
            source.Reputations.Select(EmailAddressReputationDto.Parse));
    }
}