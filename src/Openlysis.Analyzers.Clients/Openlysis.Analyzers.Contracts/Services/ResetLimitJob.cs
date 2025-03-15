using Openlysis.Analyzers.Contracts.Enums;
using Openlysis.Analyzers.Contracts.Interfaces;

using Quartz;

namespace Openlysis.Analyzers.Contracts.Services;

/// <summary>
/// Represents a job that resets request limits based on the specified period.
/// </summary>
public class ResetLimitJob : IJob
{
    public const string JobDataMapKey = "RequestLimitPeriod";
    
    private readonly IRequestLimitTracker _requestLimitTracker;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ResetLimitJob"/> class.
    /// </summary>
    /// <param name="requestLimitTracker">The request limit manager.</param>
    public ResetLimitJob(IRequestLimitTracker requestLimitTracker)
    {
        _requestLimitTracker = requestLimitTracker;
    }
    
    /// <inheritdoc/>
    public Task Execute(IJobExecutionContext context)
    {
        RequestLimitPeriod limitPeriod = (RequestLimitPeriod) context.MergedJobDataMap.GetInt(JobDataMapKey);
        
        if (limitPeriod is RequestLimitPeriod.Day)
        {
            _requestLimitTracker.ResetDailyRequestCount();
        }
        else if (limitPeriod is RequestLimitPeriod.Month)
        {
            _requestLimitTracker.ResetMonthlyRequestCount();
        }

        return Task.CompletedTask;
    }
}