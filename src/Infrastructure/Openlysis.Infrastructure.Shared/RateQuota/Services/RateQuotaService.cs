using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Configuration;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.RateQuota.Models;

namespace Openlysis.Infrastructure.Shared.RateQuota.Services;

/// <summary>
/// Tracks rate and quota limits of external APIs according to <see cref="RateQuotaOptions"/>.
/// </summary>
public sealed class RateQuotaService : IRateQuotaService, IDisposable, IAsyncDisposable
{
    /// <inheritdoc/>
    public event Action<HashSet<AnalysisEndpointType>, RateQuotaPeriod>? LimitExceed;

    /// <inheritdoc/>
    public event Action<RateQuotaTracker>? LimitRecovered;

    private readonly TimeProvider _timeProvider;
    private readonly List<RateQuotaTracker> _trackers = [];
    private readonly ITimer _refreshTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateQuotaService"/> class.
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
        IOptionsMonitor<RateQuotaOptions> rateQuotaOptions,
        TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        foreach (var key in rateQuotaOptionKeys)
        {
            var tracker = new RateQuotaTracker(key, rateQuotaOptions);
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
    public void Track(AnalysisEndpointType endpointType)
    {
        RateQuotaTracker[] trackers = _trackers
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
        foreach (RateQuotaTracker tracker in _trackers)
        {
            tracker.ResetDailyQuotaUsage();
        }
    }

    /// <inheritdoc/>
    public void RestoreMonthlyUsage()
    {
        foreach (RateQuotaTracker tracker in _trackers)
        {
            tracker.ResetMonthlyQuotaUsage();
        }
    }

    /// <inheritdoc/>
    public bool AreAvailable(HashSet<AnalysisEndpointType> endpointTypes)
    {
        RateQuotaTracker[] trackers = endpointTypes
            .SelectMany(e => _trackers.Where(t => t.EndpointTypes.Contains(e)))
            .ToArray();

        return trackers.All(t => t.HasAvailableCapacity());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _refreshTimer.Dispose();
        foreach (RateQuotaTracker tracker in _trackers)
        {
            tracker.CapacityExhausted -= OnTrackerCapacityExhausted;
            tracker.CapacityRestored -= OnTrackerCapacityRestored;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _refreshTimer.DisposeAsync();
        foreach (RateQuotaTracker tracker in _trackers)
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
        foreach (RateQuotaTracker tracker in _trackers)
        {
            tracker.RefreshRate(nowTime);
        }
    }

    private void OnTrackerCapacityExhausted(
        HashSet<AnalysisEndpointType> endpointTypes,
        RateQuotaPeriod rateQuotaPeriod)
    {
        LimitExceed?.Invoke(endpointTypes, rateQuotaPeriod);
    }

    private void OnTrackerCapacityRestored(RateQuotaTracker tracker)
    {
        LimitRecovered?.Invoke(tracker);
    }
}