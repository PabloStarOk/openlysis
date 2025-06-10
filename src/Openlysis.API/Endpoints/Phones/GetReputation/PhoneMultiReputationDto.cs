using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Data transfer object for <see cref="PhoneMultiReputation"/>.
/// </summary>
/// <param name="Id">The unique identifier of the multi-reputation.</param>
/// <param name="EvaluationDate">The date when the reputation was evaluated.</param>
/// <param name="FinalVerdict">The final verdict assigned to the phone number based on multiple reputations.</param>
/// <param name="FinalThreatZone">The final threat zone classification for the phone number.</param>
/// <param name="PhoneNumber">The phone number associated with the multi-reputation.</param>
/// <param name="Reputations">A collection of <see cref="PhoneReputationDto"/>.</param>
public record PhoneMultiReputationDto(
    string Id,
    DateTime EvaluationDate,
    Verdict FinalVerdict,
    ThreatZone FinalThreatZone,
    string PhoneNumber,
    IEnumerable<PhoneReputationDto> Reputations)
{
    /// <summary>
    /// Parses a <see cref="PhoneMultiReputation"/> object into a <see cref="PhoneMultiReputationDto"/>.
    /// </summary>
    /// <param name="reputation">The <see cref="PhoneMultiReputation"/> object to parse.</param>
    /// <returns>A new instance of <see cref="PhoneMultiReputationDto"/>.</returns>
    public static PhoneMultiReputationDto Parse(PhoneMultiReputation reputation)
    {
        return new PhoneMultiReputationDto(
            reputation.Id.Value.ToString(),
            reputation.ReputationEvaluationDate,
            reputation.FinalVerdict,
            reputation.FinalThreatZone,
            reputation.PhoneNumber,
            reputation.Reputations.Select(PhoneReputationDto.Parse));
    }
}