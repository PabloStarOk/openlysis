using System.Collections.Concurrent;

using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Configuration;
using Openlysis.Analyzers.Contracts.Enums;
using Openlysis.Analyzers.Contracts.Interfaces;

namespace Openlysis.Analyzers.Contracts.Services;

/// <summary>
/// Manages request limits and triggers events when limits are reached.
/// </summary>
public class RequestLimitManager : IRequestLimitManager, IDisposable, IAsyncDisposable
{
    public event EventHandler<RequestLimitPeriod>? OnLimitReached;
    public event EventHandler<RequestLimitPeriod>? OnRateReduced;

    private readonly IOptionsMonitor<RequestLimitOptions> _limitOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ITimer _timer;

    private readonly ConcurrentQueue<DateTimeOffset> _currentRequestsPerMinute = [];
    private readonly ConcurrentQueue<DateTimeOffset> _currentRequestsPerHour = [];

    private int _currentRequestsPerDay;
    private int _currentRequestsPerMonth;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLimitManager"/> class.
    /// </summary>
    /// <param name="limitOptions">The options monitor for request limit configurations.</param>
    /// <param name="timeProvider">The time provider used to create timers.</param>
    public RequestLimitManager(
        IOptionsMonitor<RequestLimitOptions> limitOptions,
        TimeProvider timeProvider)
    {
        _limitOptions = limitOptions;
        _timeProvider = timeProvider;
        _timer = _timeProvider.CreateTimer(
            UpdateRequestQueues,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(_limitOptions.CurrentValue.UpdateFrequencyMs));
    }
    
    /// <inheritdoc/>
    public void AddRequest()
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        RequestLimitOptions options = _limitOptions.CurrentValue;

        if (options.RatePerMinute > 0)
        {
            _currentRequestsPerMinute.Enqueue(now);
            CheckIfLimitReached(
                _currentRequestsPerMinute.Count,
                options.RatePerMinute,
                RequestLimitPeriod.Minute);   
        }

        if (options.RatePerHour > 0)
        {
            _currentRequestsPerHour.Enqueue(now);
            CheckIfLimitReached(
                _currentRequestsPerHour.Count,
                options.RatePerHour,
                RequestLimitPeriod.Hour);
        }
        
        _currentRequestsPerDay++;
        CheckIfLimitReached(
            _currentRequestsPerDay,
            options.QuotaPerDay,
            RequestLimitPeriod.Day);
        
        _currentRequestsPerMonth++;
        CheckIfLimitReached(
            _currentRequestsPerMonth,
            options.QuotaPerMonth,
            RequestLimitPeriod.Month);
    }

    /// <inheritdoc/>
    public void ResetDailyRequestCount()
    {
        _currentRequestsPerDay = 0;
    }

    /// <inheritdoc/>
    public void ResetMonthlyRequestCount()
    {
        _currentRequestsPerMonth = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
    
    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _timer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Updates the request queues by removing expired requests.
    /// </summary>
    /// <param name="state">The state object passed to the timer callback.</param>
    private void UpdateRequestQueues(object? state)
    {
        DequeueExpiredRequests(
            _currentRequestsPerMinute,
            TimeSpan.FromMinutes(1),
            RequestLimitPeriod.Minute);
        
        DequeueExpiredRequests(
            _currentRequestsPerHour,
            TimeSpan.FromHours(1),
            RequestLimitPeriod.Hour);
    }

    /// <summary>
    /// Removes expired requests from the queue and triggers the OnRateReduced event if a request is removed.
    /// </summary>
    /// <param name="requestQueue">The queue of request timestamps.</param>
    /// <param name="expirationPeriod">The period after which requests are considered expired.</param>
    /// <param name="limitPeriod">The period for which the rate limit is being checked (e.g., minute, hour).</param>
    private void DequeueExpiredRequests(
        ConcurrentQueue<DateTimeOffset> requestQueue,
        TimeSpan expirationPeriod,
        RequestLimitPeriod limitPeriod)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        if (!requestQueue.TryPeek(out DateTimeOffset requestTimestamp)
            || now - requestTimestamp < expirationPeriod)
        {
            return;
        }

        requestQueue.TryDequeue(out DateTimeOffset _);
        OnRateReduced?.Invoke(this, limitPeriod);
    }

    /// <summary>
    /// Checks if the current number of requests has reached or exceeded the target limit.
    /// If the limit is reached, triggers the OnLimitReached event.
    /// </summary>
    /// <param name="current">The current number of requests.</param>
    /// <param name="target">The target limit for the number of requests.</param>
    /// <param name="limitPeriod">The period for which the rate limit is being checked (e.g., minute, hour).</param>
    private void CheckIfLimitReached(int current, int target, RequestLimitPeriod limitPeriod)
    {
        if (target > 0 && current >= target)
        {
            OnLimitReached?.Invoke(this, limitPeriod);
        }
    }
}