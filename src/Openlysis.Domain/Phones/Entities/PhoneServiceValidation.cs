using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Models;
using Openlysis.Domain.Common.ServiceAnalyses.Mappings;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones.ValueObjects;

namespace Openlysis.Domain.Phones.Entities;

/// <summary>
/// Represents a phone service validation entity.
/// </summary>
public class PhoneServiceValidation : Entity<Id>
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the verdict of the service validation.
    /// </summary>
    public Verdict Verdict { get; }

    /// <summary>
    /// Gets the threat zone associated with the verdict.
    /// </summary>
    public ThreatZone ThreatZone { get; }

    /// <summary>
    /// Gets the phone information associated with the service validation.
    /// </summary>
    public PhoneInfo PhoneInfo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneServiceValidation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the phone service validation.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The verdict of the service validation.</param>
    /// <param name="threatZone">The threat zone associated with the verdict.</param>
    /// <param name="phoneInfo">The phone information associated with the service validation.</param>
    private PhoneServiceValidation(
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
    private PhoneServiceValidation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="PhoneServiceValidation"/>.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The verdict of the service validation.</param>
    /// <param name="phoneInfo">The phone information associated with the service validation.</param>
    /// <returns>A new instance of <see cref="PhoneServiceValidation"/>.</returns>
    public static PhoneServiceValidation Create(
        string serviceName,
        Verdict verdict,
        PhoneInfo phoneInfo)
    {
        Id id = Id.CreateUnique();
        ThreatZone threatZone = ThreatZoneMapping.Map[verdict];
        return new PhoneServiceValidation(
            id,
            serviceName,
            verdict,
            threatZone,
            phoneInfo);
    }
}