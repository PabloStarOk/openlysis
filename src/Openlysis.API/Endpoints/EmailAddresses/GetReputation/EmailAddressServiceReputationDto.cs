using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.EmailAddresses.Entities;

namespace Openlysis.API.Endpoints.EmailAddresses.GetReputation;

/// <summary>
/// Represents the reputation details of an email address service.
/// </summary>
/// <param name="Id">The unique identifier of the email address service.</param>
/// <param name="ServiceName">The name of the email address service.</param>
/// <param name="Verdict">The verdict associated with the email address service.</param>
/// <param name="ThreatZone">The threat zone classification of the email address service.</param>
/// <param name="IsDisposable">Indicates whether the email address is disposable.</param>
/// <param name="IsRiskyTld">Indicates whether the email address has a risky top-level domain (TLD).</param>
public record EmailAddressServiceReputationDto(
    string Id,
    string ServiceName,
    Verdict Verdict,
    ThreatZone ThreatZone,
    bool? IsDisposable,
    bool? IsRiskyTld)
{
    /// <summary>
    /// Parses an <see cref="EmailAddressServiceReputation"/> object into an <see cref="EmailAddressServiceReputationDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="EmailAddressServiceReputation"/> object to parse.</param>
    /// <returns>An instance of <see cref="EmailAddressServiceReputationDto"/>.</returns>
    public static EmailAddressServiceReputationDto Parse(
        EmailAddressServiceReputation source)
    {
        return new EmailAddressServiceReputationDto(
            source.Id.Value.ToString(),
            source.ServiceName,
            source.Verdict,
            source.ThreatZone,
            source.IsDisposable,
            source.IsRiskyTld);
    }
}