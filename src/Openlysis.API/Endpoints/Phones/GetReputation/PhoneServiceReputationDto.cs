using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Domain.Phones.ValueObjects;

namespace Openlysis.API.Endpoints.Phones.GetReputation;

/// <summary>
/// Represents the reputation details of a phone service.
/// </summary>
/// <param name="ServiceName">The name of the phone service.</param>
/// <param name="Verdict">The verdict associated with the phone service.</param>
/// <param name="ThreatZone">The threat zone classification of the phone service.</param>
/// <param name="PhoneInfo">Detailed information about the phone.</param>
public record PhoneServiceReputationDto(
    string ServiceName,
    Verdict Verdict,
    ThreatZone ThreatZone,
    PhoneInfo PhoneInfo)
{
    /// <summary>
    /// Converts a <see cref="PhoneServiceReputation"/> instance to a <see cref="PhoneServiceReputationDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="PhoneServiceReputation"/> instance to convert.</param>
    /// <returns>A new <see cref="PhoneServiceReputationDto"/> instance with data from the source.</returns>
    public static PhoneServiceReputationDto Parse(PhoneServiceReputation source)
    {
        return new PhoneServiceReputationDto(
            source.ServiceName,
            source.Verdict,
            source.ThreatZone,
            source.PhoneInfo);
    }
}