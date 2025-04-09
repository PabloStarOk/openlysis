using Openlysis.Infrastructure.Shared.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.RateQuota.Models;

namespace Openlysis.Infrastructure.Shared.RateQuota.Abstractions;

/// <summary>
/// Define a tracker of requests sent to an external service.
/// </summary>
/// <typeparam name="TEnum">The type of enumeration used to specify endpoint types.</typeparam>
public interface IRateQuotaService<TEnum>
    where TEnum : Enum
{
    /// <summary>
    /// Event triggered when the request capacity is exhausted.
    /// </summary>
    public event Action<HashSet<TEnum>, RateQuotaPeriod> LimitExceed;

    /// <summary>
    /// Event triggered when the request capacity is restored.
    /// </summary>
    public event Action<RateQuotaTracker<TEnum>>? LimitRecovered;

    /// <summary>
    /// Tracks the request limit for a specific endpoint type.
    /// </summary>
    /// <param name="endpointType">The type of analysis endpoint to track.</param>
    public void Track(TEnum endpointType);

    /// <summary>
    /// Checks if the request limits are available for the specified endpoint types.
    /// </summary>
    /// <param name="endpointTypes">The set of analysis endpoint types to check.</param>
    /// <returns>True if the endpoints can receive requests.</returns>
    public bool AreAvailable(HashSet<TEnum> endpointTypes);
}