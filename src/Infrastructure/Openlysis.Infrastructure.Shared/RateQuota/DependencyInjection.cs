using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Configuration;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.RateQuota.Services;

using Quartz;

namespace Openlysis.Infrastructure.Shared.RateQuota;

/// <summary>
/// Provides extension methods for dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the <see cref="IRateQuotaService{TEnum}"/> service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumeration used for rate quota options.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to retrieve the configuration settings from.</param>
    /// <param name="serviceKey">The key used to identify the <see cref="IRateQuotaService{TEnum}"/> service and configured <see cref="RateQuotaOptions{TEnum}"/>.</param>
    /// <param name="configSectionName">The name of the parent section in the configuration.</param>
    public static void AddRateQuotaService<TEnum>(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceKey,
        string configSectionName)
        where TEnum : Enum
    {
        // Get options.
        var limitTrackerOptionsSection = configuration
            .GetRequiredSection(configSectionName)
            .GetRequiredSection(LimitTrackerOptions.SectionName);

        var rateQuotaOptionsSection = configuration
            .GetRequiredSection(configSectionName)
            .GetRequiredSection(LimitTrackerOptions.SectionName)
            .GetRequiredSection(RateQuotaOptions<TEnum>.SectionName);

        ArgumentNullException.ThrowIfNull(limitTrackerOptionsSection);
        ArgumentNullException.ThrowIfNull(rateQuotaOptionsSection);

        // Add options.
        services.AddOptionsWithValidateOnStart<LimitTrackerOptions>(serviceKey)
            .Bind(limitTrackerOptionsSection)
            .ValidateDataAnnotations();

        List<string> rateQuotaOptionKeys = [];
        foreach (var childSection in rateQuotaOptionsSection.GetChildren())
        {
            string key = $"{configSectionName}{childSection.Key}";
            services.AddOptionsWithValidateOnStart<RateQuotaOptions<TEnum>>(key)
                .Bind(childSection)
                .ValidateDataAnnotations();
            rateQuotaOptionKeys.Add(key);
        }

        services.AddSingleton<RateQuotaOptionsValidator<TEnum>>();

        // Get options monitor
        IOptionsMonitor<LimitTrackerOptions> limitTrackerOptions;
        IOptionsMonitor<RateQuotaOptions<TEnum>> rateQuotaOptions;
        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            limitTrackerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<LimitTrackerOptions>>();
            rateQuotaOptions = serviceProvider.GetRequiredService<IOptionsMonitor<RateQuotaOptions<TEnum>>>();
        }

        // Add limit tracker
        var limitTracker = new RateQuotaService<TEnum>(
            serviceKey,
            limitTrackerOptions,
            rateQuotaOptionKeys.ToArray(),
            rateQuotaOptions,
            TimeProvider.System);
        services.AddSingleton<IRateQuotaService<TEnum>>(limitTracker);
        services.AddKeyedSingleton<IRateQuotaService<TEnum>>(serviceKey, limitTracker);
    }

    /// <summary>
    /// Adds Quartz jobs for resetting daily and quota usage.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the jobs to.</param>
    public static void AddLimitTrackerJobs(this IServiceCollection services)
    {
        services.AddQuartz(
            q =>
            {
                // Add job
                q.SchedulerId = "AnalyzerScheduler";
                q.SchedulerName = "AnalyzerScheduler";
                var jobKey = new JobKey("DailyResetJob");
                q.AddJob<ResetUsageQuotaJob>(jobKey);

                // Add daily trigger
                CronScheduleBuilder dailySchedule = CronScheduleBuilder
                    .DailyAtHourAndMinute(hour: 0, minute: 0)
                    .InTimeZone(TimeZoneInfo.Utc)
                    .WithMisfireHandlingInstructionFireAndProceed();
                q.AddTrigger(trigger => trigger
                    .ForJob(jobKey)
                    .WithIdentity("QuotaDailyResetTrigger")
                    .WithDescription("Resets daily usage quotas at midnight of each day.")
                    .UsingJobData(ResetUsageQuotaJob.JobDataMapKey, RateQuotaPeriod.Day.ToString())
                    .WithSchedule(dailySchedule)
                    .StartNow());

                // Add monthly trigger
                CronScheduleBuilder monthlySchedule = CronScheduleBuilder
                    .MonthlyOnDayAndHourAndMinute(dayOfMonth: 1, hour: 0, minute: 0)
                    .InTimeZone(TimeZoneInfo.Utc)
                    .WithMisfireHandlingInstructionFireAndProceed();
                q.AddTrigger(trigger => trigger
                    .ForJob(jobKey)
                    .WithIdentity("QuotaMonthlyResetTrigger")
                    .WithDescription("Resets monthly usage quotas at midnight on the 1st day of each month.")
                    .UsingJobData(ResetUsageQuotaJob.JobDataMapKey, RateQuotaPeriod.Month.ToString())
                    .WithSchedule(monthlySchedule)
                    .StartNow());
            });

        services.AddQuartzHostedService(
            options =>
            {
                options.WaitForJobsToComplete = false;
            });
    }
}