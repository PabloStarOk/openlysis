using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.EmailAddresses;

namespace Openlysis.API.Endpoints.EmailAddresses.GetReputation;

/// <summary>
/// Represents the reputation details of an email address, including its evaluation date,
/// average verdict, average threat zone, and service-specific reputations.
/// </summary>
/// <param name="Id">The unique identifier of the email address.</param>
/// <param name="ReputationEvaluationDate">The date when the reputation was evaluated.</param>
/// <param name="AverageVerdict">The average verdict of the email address reputation.</param>
/// <param name="AverageThreatZone">The average threat zone of the email address reputation.</param>
/// <param name="ServiceReputations">An array of service-specific reputation details.</param>
public record EmailAddressMultiReputationDto(
    string Id,
    DateTime ReputationEvaluationDate,
    Verdict AverageVerdict,
    ThreatZone AverageThreatZone,
    string EmailAddress,
    EmailAddressServiceReputationDto[] ServiceReputations)
{
    /// <summary>
    /// Converts an <see cref="EmailAddressMultiReputation"/> instance to an <see cref="EmailAddressMultiReputationDto"/>.
    /// </summary>
    /// <param name="source">The source object containing email address reputation details.</param>
    /// <returns>An instance of <see cref="EmailAddressMultiReputationDto"/> representing the converted data.</returns>
    public static EmailAddressMultiReputationDto Parse(
        EmailAddressMultiReputation source)
    {
        EmailAddressServiceReputationDto[] serviceReputations = source.ServicesReputations
            .Select(EmailAddressServiceReputationDto.Parse)
            .ToArray();

        return new EmailAddressMultiReputationDto(
            source.Id.Value.ToString(),
            source.ReputationEvaluationDate,
            source.AverageVerdict,
            source.AverageThreatZone,
            source.EmailAddress,
            serviceReputations);
    }
}