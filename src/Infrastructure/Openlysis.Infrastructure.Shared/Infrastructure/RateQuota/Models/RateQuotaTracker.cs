using System.Collections.Concurrent;

using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Enums;

namespace Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Models;

/// <summary>
/// Manages the rate limits and quota usage for different periods (minute, hour, day, month).
/// </summary>
/// <typeparam name="TEnum">The type of the enumeration used for endpoint types.</typeparam>
public class RateQuotaTracker<TEnum>
    where TEnum : Enum
{
    /// <summary>
    /// Occurs when the capacity is exhausted for the specified endpoint types and period.
    /// </summary>
    public event Action<HashSet<TEnum>, RateQuotaPeriod>? CapacityExhausted;

    /// <summary>
    /// Occurs when the capacity is restored for the specified endpoint types and period.
    /// </summary>
    public event Action<RateQuotaTracker<TEnum>>? CapacityRestored;

    /// <summary>
    /// Gets the type of the analysis endpoint which this class tracks for.
    /// </summary>
    public HashSet<TEnum> EndpointTypes =>
        _rateQuotaOptions.Get(_optionsKey).EndpointTypes;

    private readonly string _optionsKey;
    private readonly IOptionsMonitor<RateQuotaOptions<TEnum>> _rateQuotaOptions;
    private readonly ConcurrentQueue<DateTimeOffset> _minuteRateWindow = [];
    private readonly ConcurrentQueue<DateTimeOffset> _hourlyRateWindow = [];
    private int _dailyQuotaUsage;
    private int _monthlyQuotaUsage;
    private bool _isMinuteCapacityExhausted;
    private bool _isHourlyCapacityExhausted;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateQuotaTracker{TEnum}"/> class.
    /// </summary>
    /// <param name="optionsKey">The key to retrieve the rate limit options.</param>
    /// <param name="rateQuotaOptions">The options monitor for rate and quota limit configurations.</param>
    /// <remarks>
    /// The <paramref name="optionsKey"/> is used to fetch the specific rate and quota configurations
    /// from the <paramref name="rateQuotaOptions"/> monitor.
    /// </remarks>
    public RateQuotaTracker(
        string optionsKey,
        IOptionsMonitor<RateQuotaOptions<TEnum>> rateQuotaOptions)
    {
        _optionsKey = optionsKey;
        _rateQuotaOptions = rateQuotaOptions;
    }

    /// <summary>
    /// Adds a request to the tracking queues and increments the daily and monthly request counts.
    /// </summary>
    /// <param name="timeProvider">The time provider to get the current time.</param>
    public void TrackRequest(TimeProvider timeProvider)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        _minuteRateWindow.Enqueue(now);
        _hourlyRateWindow.Enqueue(now);
        _dailyQuotaUsage++;
        _monthlyQuotaUsage++;
        EvaluateExhaustedCapacity();
    }

    /// <summary>
    /// Refreshes the rate windows by removing expired requests from the minute and hourly rate windows.
    /// </summary>
    /// <param name="now">The current time to compare against the request times.</param>
    public void RefreshRate(DateTimeOffset now)
    {
        RemoveExpiredRequests(now, _minuteRateWindow, TimeSpan.FromMinutes(1));
        RemoveExpiredRequests(now, _hourlyRateWindow, TimeSpan.FromHours(1));
        EvaluateRestoredRate();
    }

    /// <summary>
    /// Resets the daily quota usage to zero.
    /// </summary>
    public void ResetDailyQuotaUsage()
    {
        _dailyQuotaUsage = 0;
        CapacityRestored?.Invoke(this);
    }

    /// <summary>
    /// Resets the monthly quota usage to zero.
    /// </summary>
    public void ResetMonthlyQuotaUsage()
    {
        _monthlyQuotaUsage = 0;
        CapacityRestored?.Invoke(this);
    }

    /// <summary>
    /// Gets a value indicating whether there is available capacity for requests.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there is available capacity; otherwise, <c>false</c>.
    /// </returns>
    public bool HasAvailableCapacity()
    {
        RateQuotaOptions<TEnum> options = _rateQuotaOptions.Get(_optionsKey);

        bool hasMinuteRateCapacity =
            options.MinuteRate <= 0 || _minuteRateWindow.Count < options.MinuteRate;

        bool hasHourlyRateCapacity =
            options.HourlyRate <= 0 || _hourlyRateWindow.Count < options.HourlyRate;

        bool hasDailyQuotaRemaining =
            options.DailyQuota <= 0 || _dailyQuotaUsage < options.DailyQuota;

        bool hasMonthlyQuotaRemaining =
            options.MonthlyQuota <= 0 || _monthlyQuotaUsage < options.MonthlyQuota;

        return hasMinuteRateCapacity && hasHourlyRateCapacity
            && hasDailyQuotaRemaining && hasMonthlyQuotaRemaining;
    }

    /// <summary>
    /// Removes expired requests from the tracking queue based on the specified expiration period.
    /// </summary>
    /// <param name="nowTime">The current time to compare against the request times.</param>
    /// <param name="trackedRequests">The queue of tracked request times.</param>
    /// <param name="expirationPeriod">The period after which requests are considered expired.</param>
    private static void RemoveExpiredRequests(
        DateTimeOffset nowTime,
        ConcurrentQueue<DateTimeOffset> trackedRequests,
        TimeSpan expirationPeriod)
    {
        while (trackedRequests.TryPeek(out DateTimeOffset requestTime))
        {
            TimeSpan timeDifference = nowTime - requestTime;
            if (timeDifference < expirationPeriod)
            {
                return;
            }

            trackedRequests.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Evaluates and triggers events for exhausted capacity when the rate limits are reached.
    /// </summary>
    private void EvaluateExhaustedCapacity()
    {
        RateQuotaOptions<TEnum> options = _rateQuotaOptions.Get(_optionsKey);

        if (options.MonthlyQuota > 0
            && _monthlyQuotaUsage >= options.MonthlyQuota)
        {
            CapacityExhausted?.Invoke(EndpointTypes, RateQuotaPeriod.Month);
        }

        if (options.DailyQuota > 0
            && _dailyQuotaUsage >= options.DailyQuota)
        {
            CapacityExhausted?.Invoke(EndpointTypes, RateQuotaPeriod.Day);
        }

        if (options.HourlyRate > 0
            && _hourlyRateWindow.Count >= options.HourlyRate)
        {
            _isHourlyCapacityExhausted = true;
            CapacityExhausted?.Invoke(EndpointTypes, RateQuotaPeriod.Hour);
        }

        if (options.MinuteRate > 0
            && _minuteRateWindow.Count >= options.MinuteRate)
        {
            _isMinuteCapacityExhausted = true;
            CapacityExhausted?.Invoke(EndpointTypes, RateQuotaPeriod.Minute);
        }
    }

    /// <summary>
    /// Evaluates and triggers events for restored capacity when the rate limits are no longer exhausted.
    /// </summary>
    private void EvaluateRestoredRate()
    {
        if (!HasAvailableCapacity())
        {
            return;
        }

        if (_isHourlyCapacityExhausted)
        {
            _isHourlyCapacityExhausted = false;
            CapacityRestored?.Invoke(this);
        }

        if (_isMinuteCapacityExhausted)
        {
            _isMinuteCapacityExhausted = false;
            CapacityRestored?.Invoke(this);
        }
    }
}
