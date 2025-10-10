using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Common.Entities;

/// <summary>
/// A base entity that represents the reputation returned by a service.
/// </summary>
public abstract class Reputation : Entity<GlobalId>
{
    /// <summary>
    /// Gets the name of the service that returned the reputation.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the verdict of the reputation.
    /// </summary>
    public Verdict Verdict { get; }

    /// <summary>
    /// Gets the threat zone associated with the verdict.
    /// </summary>
    public ThreatZone ThreatZone { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Reputation"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the service reputation entity.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="verdict">The verdict of the reputation.</param>
    /// <param name="threatZone">The threat zone associated with the verdict.</param>
    protected Reputation(
        GlobalId id,
        string serviceName,
        Verdict verdict,
        ThreatZone threatZone)
        : base(id)
    {
        ServiceName = serviceName;
        Verdict = verdict;
        ThreatZone = threatZone;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="Reputation"/> class for EF Core.
    /// This constructor is protected to prevent direct instantiation outside of EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor must not be used in the application code.
    /// </remarks>
    protected Reputation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618
}