using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Represents the response for retrieving the reputation of a phone number.
/// </summary>
/// <param name="AssessmentDate">The date when the assessment was performed.</param>
/// <param name="AverageVerdict">The average verdict of the phone number's reputation.</param>
/// <param name="AverageThreatZone">The average threat zone associated with the phone number.</param>
/// <param name="ServicesReputations">An array of reputations for individual phone services.</param>
public record PhoneMultiReputationDto(
    DateTime AssessmentDate,
    Verdict AverageVerdict,
    ThreatZone AverageThreatZone,
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
            reputation.AverageVerdict,
            reputation.AverageThreatZone,
            reputation.ServicesReputations
                .Select(PhoneServiceReputationDto.Parse)
                .ToArray());
    }
}