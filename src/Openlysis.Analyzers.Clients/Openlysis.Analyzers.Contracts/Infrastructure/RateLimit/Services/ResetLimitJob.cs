using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;

using Quartz;

namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Services;

/// <summary>
/// Represents a job that resets request limits based on the specified period.
/// </summary>
public class ResetLimitJob : IJob
{
    /// <summary>
    /// The key used to retrieve the request limit period from the job data map.
    /// </summary>
    public const string JobDataMapKey = "RequestLimitPeriod";

    private readonly IEnumerable<IRequestLimitTracker> _limitTrackers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetLimitJob"/> class.
    /// </summary>
    /// <param name="limitTrackers">A collection of <see cref="IRequestLimitTracker"/>.</param>
    public ResetLimitJob(IEnumerable<IRequestLimitTracker> limitTrackers)
    {
        _limitTrackers = limitTrackers;
    }

    /// <inheritdoc/>
    public Task Execute(IJobExecutionContext context)
    {
        var limitPeriod = (RequestLimitPeriod)context.MergedJobDataMap.GetInt(JobDataMapKey);
        var limitTrackersList = _limitTrackers.ToList();

        if (limitPeriod is RequestLimitPeriod.Day)
        {
            limitTrackersList
                .ForEach(t => t.ResetDailyRequestCount());
        }
        else if (limitPeriod is RequestLimitPeriod.Month)
        {
            limitTrackersList
                .ForEach(t => t.ResetMonthlyRequestCount());
        }

        return Task.CompletedTask;
    }
}