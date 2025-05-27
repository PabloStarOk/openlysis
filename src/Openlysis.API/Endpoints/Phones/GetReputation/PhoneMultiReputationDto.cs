using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Data transfer object for <see cref="PhoneMultiReputation"/>.
/// </summary>
/// <param name="EvaluationDate">The date when the assessment was performed.</param>
/// <param name="FinalVerdict">The final verdict of the phone number's reputation.</param>
/// <param name="FinalThreatZone">The final threat zone associated with the phone number.</param>
/// <param name="PhoneNumber">The phone number associated with the reputation.</param>
/// <param name="Reputations">An array of reputation details from different sources or services.</param>
public record PhoneMultiReputationDto(
    DateTime EvaluationDate,
    Verdict FinalVerdict,
    ThreatZone FinalThreatZone,
    string PhoneNumber,
    PhoneReputationDto[] Reputations)
{
    /// <summary>
    /// Parses a <see cref="PhoneMultiReputation"/> object into a <see cref="PhoneMultiReputationDto"/>.
    /// </summary>
    /// <param name="reputation">The <see cref="PhoneMultiReputation"/> object to parse.</param>
    /// <returns>A new instance of <see cref="PhoneMultiReputationDto"/>.</returns>
    public static PhoneMultiReputationDto Parse(PhoneMultiReputation reputation)
    {
        return new PhoneMultiReputationDto(
            reputation.ReputationEvaluationDate,
            reputation.FinalVerdict,
            reputation.FinalThreatZone,
            reputation.PhoneNumber,
            reputation.Reputations.Select(PhoneReputationDto.Parse).ToArray());
    }
}