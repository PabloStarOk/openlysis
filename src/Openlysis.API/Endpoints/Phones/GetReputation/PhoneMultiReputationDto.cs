using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Represents the response for retrieving the reputation of a phone number.
/// </summary>
/// <param name="AssessmentDate">The date when the assessment was performed.</param>
/// <param name="FinalVerdict">The final verdict of the phone number's reputation.</param>
/// <param name="FinalThreatZone">The final threat zone associated with the phone number.</param>
/// <param name="PhoneNumber">The phone number associated with the reputation.</param>
/// <param name="ServicesReputations">An array of reputations for individual phone services.</param>
public record PhoneMultiReputationDto(
    DateTime AssessmentDate,
    Verdict FinalVerdict,
    ThreatZone FinalThreatZone,
    string PhoneNumber,
    PhoneServiceReputationDto[] ServicesReputations)
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
            reputation.ServicesReputations
                .Select(PhoneServiceReputationDto.Parse)
                .ToArray());
    }
}