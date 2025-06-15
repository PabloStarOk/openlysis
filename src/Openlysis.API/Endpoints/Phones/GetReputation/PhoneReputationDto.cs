using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Domain.Phones.ValueObjects;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Data transfer object for <see cref="PhoneReputation"/>.
/// </summary>
/// <param name="Id">The unique identifier of the reputation.</param>
/// <param name="ServiceName">The name of the service that returned the multi-reputation.</param>
/// <param name="Verdict">The verdict of the multi-reputation.</param>
/// <param name="ThreatZone">The threat zone classification of the multi-reputation.</param>
/// <param name="PhoneInfo">Detailed information about the phone.</param>
public record PhoneReputationDto(
    string Id,
    string ServiceName,
    Verdict Verdict,
    ThreatZone ThreatZone,
    PhoneInfo PhoneInfo)
{
    /// <summary>
    /// Converts a <see cref="PhoneReputation"/> instance to a <see cref="PhoneReputationDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="PhoneReputation"/> instance to convert.</param>
    /// <returns>A new <see cref="PhoneReputationDto"/> instance with data from the source.</returns>
    public static PhoneReputationDto Parse(PhoneReputation source)
    {
        return new PhoneReputationDto(
            source.Id.Value.ToString(),
            source.ServiceName,
            source.Verdict,
            source.ThreatZone,
            source.PhoneInfo);
    }
}