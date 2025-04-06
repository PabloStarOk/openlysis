using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;

using Quartz;

namespace Openlysis.Infrastructure.Shared.RateQuota.Services;

/// <summary>
/// Represents a job that resets the usage quota for rate limiting.
/// </summary>
public class ResetUsageQuotaJob : IJob
{
    /// <summary>
    /// The key used to retrieve the request limit period from the job data map.
    /// </summary>
    public const string JobDataMapKey = "RateQuotaPeriod";

    private readonly IEnumerable<IRateQuotaService> _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetUsageQuotaJob"/> class.
    /// </summary>
    /// <param name="services">A collection of <see cref="IRateQuotaService"/>.</param>
    public ResetUsageQuotaJob(IEnumerable<IRateQuotaService> services)
    {
        _services = services;
    }

    /// <inheritdoc/>
    public Task Execute(IJobExecutionContext context)
    {
        if (!Enum.TryParse(
                context.MergedJobDataMap.GetString(JobDataMapKey),
                out RateQuotaPeriod rateQuotaPeriod))
        {
            throw new ArgumentException("Couldn't parse RateQuotaPeriod enum with the given data of the context.", nameof(context));
        }

        var limitTrackersList = _services.ToList();

        if (rateQuotaPeriod is RateQuotaPeriod.Day)
        {
            limitTrackersList
                .ForEach(t => t.RestoreDailyUsage());
        }
        else if (rateQuotaPeriod is RateQuotaPeriod.Month)
        {
            limitTrackersList
                .ForEach(t => t.RestoreMonthlyUsage());
        }

        return Task.CompletedTask;
    }
}