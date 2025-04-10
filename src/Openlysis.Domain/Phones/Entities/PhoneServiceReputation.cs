using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones.ValueObjects;

namespace Openlysis.Domain.Phones.Entities;

/// <summary>
/// Represents the reputation of a phone given by a service.
/// </summary>
public class PhoneServiceReputation : Entity<Id>
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the verdict of the service.
    /// </summary>
    public Verdict Verdict { get; }

    /// <summary>
    /// Gets the threat zone associated with the verdict.
    /// </summary>
    public ThreatZone ThreatZone { get; }

    /// <summary>
    /// Gets the phone information associated with the service.
    /// </summary>
    public PhoneInfo PhoneInfo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneServiceReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The verdict of the service.</param>
    /// <param name="threatZone">The threat zone associated with the verdict.</param>
    /// <param name="phoneInfo">The phone information associated with the service.</param>
    private PhoneServiceReputation(
        Id id,
        string serviceName,
        Verdict verdict,
        ThreatZone threatZone,
        PhoneInfo phoneInfo)
        : base(id)
    {
        ServiceName = serviceName;
        Verdict = verdict;
        ThreatZone = threatZone;
        PhoneInfo = phoneInfo;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private PhoneServiceReputation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="PhoneServiceReputation"/>.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The verdict of the reputation.</param>
    /// <param name="phoneInfo">The phone information associated with the reputation.</param>
    /// <returns>A new instance of <see cref="PhoneServiceReputation"/>.</returns>
    public static PhoneServiceReputation Create(
        string serviceName,
        Verdict verdict,
        PhoneInfo phoneInfo)
    {
        Id id = Id.CreateUnique();
        ThreatZone threatZone = ThreatZoneMapping.Map[verdict];
        return new PhoneServiceReputation(
            id,
            serviceName,
            verdict,
            threatZone,
            phoneInfo);
    }
}