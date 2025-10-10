using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.EmailAddresses.Entities;

/// <summary>
/// The reputation for an email address returned by a service.
/// </summary>
public class EmailAddressReputation : Reputation
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
    /// Initializes a new instance of the <see cref="EmailAddressReputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the reputation.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The reputation verdict of the email address.</param>
    /// <param name="threatZone">The threat zone associated with the email address.</param>
    /// <param name="isDisposable">Indicates whether the email address is disposable.</param>
    /// <param name="isRiskyTld">Indicates whether the email address has a risky top-level domain (TLD).</param>
    private EmailAddressReputation(
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
    private EmailAddressReputation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="EmailAddressReputation"/> class.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The reputation verdict of the email address.</param>
    /// <param name="isDisposable">Indicates whether the email address is disposable.</param>
    /// <param name="isRiskyTld">Indicates whether the email address has a risky top-level domain (TLD).</param>
    /// <returns>A new instance of <see cref="EmailAddressReputation"/>.</returns>
    public static EmailAddressReputation Create(
        string serviceName,
        Verdict verdict,
        bool? isDisposable,
        bool? isRiskyTld)
    {
        GlobalId globalId = GlobalId.CreateUnique();
        ThreatZone threatZone = ThreatZoneMapping.Map[verdict];

        return new EmailAddressReputation(
            globalId,
            serviceName,
            verdict,
            threatZone,
            isDisposable,
            isRiskyTld);
    }
}