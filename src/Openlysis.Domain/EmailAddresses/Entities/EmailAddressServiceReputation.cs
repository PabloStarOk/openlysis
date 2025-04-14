using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.EmailAddresses.Entities;

/// <summary>
/// Represents a service that evaluates the reputation of an email address.
/// </summary>
/// <remarks>
/// Inherits from the <see cref="ServiceReputation"/> class to provide additional
/// properties and methods specific to email address reputation evaluation.
/// </remarks>
public class EmailAddressServiceReputation : ServiceReputation
{
    /// <summary>
    /// Gets a value indicating whether the email address is disposable.
    /// </summary>
    public bool? IsDisposable { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the email address has a risky top-level domain (TLD).
    /// </summary>
    public bool? IsRiskyTld { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressServiceReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The reputation verdict of the email address.</param>
    /// <param name="threatZone">The threat zone associated with the email address.</param>
    /// <param name="isDisposable">Indicates whether the email address is disposable.</param>
    /// <param name="isRiskyTld">Indicates whether the email address has a risky top-level domain (TLD).</param>
    private EmailAddressServiceReputation(
        GlobalId id,
        string serviceName,
        Verdict verdict,
        ThreatZone threatZone,
        bool? isDisposable,
        bool? isRiskyTld)
        : base(id, serviceName, verdict, threatZone)
    {
        IsDisposable = isDisposable;
        IsRiskyTld = isRiskyTld;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private EmailAddressServiceReputation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="EmailAddressServiceReputation"/> class.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The reputation verdict of the email address.</param>
    /// <param name="isDisposable">Indicates whether the email address is disposable.</param>
    /// <param name="isRiskyTld">Indicates whether the email address has a risky top-level domain (TLD).</param>
    /// <returns>A new instance of <see cref="EmailAddressServiceReputation"/>.</returns>
    public static EmailAddressServiceReputation Create(
        string serviceName,
        Verdict verdict,
        bool? isDisposable,
        bool? isRiskyTld)
    {
        GlobalId globalId = GlobalId.CreateUnique();
        ThreatZone threatZone = ThreatZoneMapping.Map[verdict];

        return new EmailAddressServiceReputation(
            globalId,
            serviceName,
            verdict,
            threatZone,
            isDisposable,
            isRiskyTld);
    }
}