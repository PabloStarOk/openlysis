using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.EmailAddresses.Entities;

namespace Openlysis.API.Endpoints.EmailAddresses.GetReputation;

/// <summary>
/// Data transfer object for <see cref="EmailAddressReputation"/>.
/// </summary>
/// <param name="Id">The unique identifier of the reputation.</param>
/// <param name="ServiceName">The name of the service that returned the reputation.</param>
/// <param name="Verdict">The verdict of the reputation.</param>
/// <param name="ThreatZone">The threat zone classification of the reputation.</param>
/// <param name="IsDisposable">Indicates whether the email address is disposable.</param>
/// <param name="IsRiskyTld">Indicates whether the email address has a risky top-level domain (TLD).</param>
public record EmailAddressReputationDto(
    string Id,
    string ServiceName,
    Verdict Verdict,
    ThreatZone ThreatZone,
    bool? IsDisposable,
    bool? IsRiskyTld)
{
    /// <summary>
    /// Parses an <see cref="EmailAddressReputation"/> object into an <see cref="EmailAddressReputationDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="EmailAddressReputation"/> object to parse.</param>
    /// <returns>An instance of <see cref="EmailAddressReputationDto"/>.</returns>
    public static EmailAddressReputationDto Parse(
        EmailAddressReputation source)
    {
        return new EmailAddressReputationDto(
            source.Id.Value.ToString(),
            source.ServiceName,
            source.Verdict,
            source.ThreatZone,
            source.IsDisposable,
            source.IsRiskyTld);
    }
}