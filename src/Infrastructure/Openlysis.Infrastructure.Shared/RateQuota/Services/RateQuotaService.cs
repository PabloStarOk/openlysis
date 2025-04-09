using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Configuration;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.RateQuota.Models;

namespace Openlysis.Infrastructure.Shared.RateQuota.Services;

/// <summary>
/// Tracks rate and quota limits of external APIs according to <see cref="RateQuotaOptions{TEnum}"/>.
/// </summary>
/// <typeparam name="TEnum">The type of the enumeration used for endpoint types.</typeparam>
public sealed class RateQuotaService<TEnum>
    : IRateQuotaService<TEnum>, IQuotaRestorable, IDisposable, IAsyncDisposable
    where TEnum : Enum
{
    /// <inheritdoc/>
    public event Action<HashSet<TEnum>, RateQuotaPeriod>? LimitExceed;

    /// <inheritdoc/>
    public event Action<RateQuotaTracker<TEnum>>? LimitRecovered;

    private readonly TimeProvider _timeProvider;
    private readonly List<RateQuotaTracker<TEnum>> _trackers = [];
    private readonly ITimer _refreshTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateQuotaService{TEnum}"/> class.
    /// </summary>
    /// <param name="optionsInstanceName">The name of the options instance.</param>
    /// <param name="limitTrackerOptions">The options monitor for limit tracker options.</param>
    /// <param name="rateQuotaOptionKeys">The keys for rate quota options.</param>
    /// <param name="rateQuotaOptions">The options monitor for rate quota options.</param>
    /// <param name="timeProvider">The time provider.</param>
    public RateQuotaService(
        string optionsInstanceName,
        IOptionsMonitor<LimitTrackerOptions> limitTrackerOptions,
        string[] rateQuotaOptionKeys,
        IOptionsMonitor<RateQuotaOptions<TEnum>> rateQuotaOptions,
        TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        foreach (var key in rateQuotaOptionKeys)
        {
            var tracker = new RateQuotaTracker<TEnum>(key, rateQuotaOptions);
            tracker.CapacityExhausted += OnTrackerCapacityExhausted;
            tracker.CapacityRestored += OnTrackerCapacityRestored;
            _trackers.Add(tracker);
        }

        _refreshTimer = _timeProvider.CreateTimer(
            UpdateTrackers,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(limitTrackerOptions.Get(optionsInstanceName).RateWindowRefreshIntervalMs));
    }

    /// <inheritdoc/>
    public void Track(TEnum endpointType)
    {
        RateQuotaTracker<TEnum>[] trackers = _trackers
            .Where(e => e.EndpointTypes.Contains(endpointType))
            .ToArray();

        foreach (var tracker in trackers)
        {
            tracker.TrackRequest(_timeProvider);
        }
    }

    /// <inheritdoc/>
    public void RestoreDailyUsage()
    {
        foreach (RateQuotaTracker<TEnum> tracker in _trackers)
        {
            tracker.ResetDailyQuotaUsage();
        }
    }

    /// <inheritdoc/>
    public void RestoreMonthlyUsage()
    {
        foreach (RateQuotaTracker<TEnum> tracker in _trackers)
        {
            tracker.ResetMonthlyQuotaUsage();
        }
    }

    /// <inheritdoc/>
    public bool AreAvailable(HashSet<TEnum> endpointTypes)
    {
        RateQuotaTracker<TEnum>[] trackers = endpointTypes
            .SelectMany(e => _trackers.Where(t => t.EndpointTypes.Contains(e)))
            .ToArray();

        return trackers.All(t => t.HasAvailableCapacity());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _refreshTimer.Dispose();
        foreach (RateQuotaTracker<TEnum> tracker in _trackers)
        {
            tracker.CapacityExhausted -= OnTrackerCapacityExhausted;
            tracker.CapacityRestored -= OnTrackerCapacityRestored;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _refreshTimer.DisposeAsync();
        foreach (RateQuotaTracker<TEnum> tracker in _trackers)
        {
            tracker.CapacityExhausted -= OnTrackerCapacityExhausted;
            tracker.CapacityRestored -= OnTrackerCapacityRestored;
        }
    }

    /// <summary>
    /// Updates the rate quota trackers for each endpoint type.
    /// </summary>
    /// <param name="state">An optional state object passed by the timer.</param>
    private void UpdateTrackers(object? state)
    {
        DateTimeOffset nowTime = _timeProvider.GetUtcNow();
        foreach (RateQuotaTracker<TEnum> tracker in _trackers)
        {
            tracker.RefreshRate(nowTime);
        }
    }

    private void OnTrackerCapacityExhausted(
        HashSet<TEnum> endpointTypes,
        RateQuotaPeriod rateQuotaPeriod)
    {
        LimitExceed?.Invoke(endpointTypes, rateQuotaPeriod);
    }

    private void OnTrackerCapacityRestored(RateQuotaTracker<TEnum> tracker)
    {
        LimitRecovered?.Invoke(tracker);
    }
}