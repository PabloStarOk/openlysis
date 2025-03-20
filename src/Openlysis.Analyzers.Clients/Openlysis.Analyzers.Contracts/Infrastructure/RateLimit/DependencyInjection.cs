using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Configuration;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Enums;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Services;

using Quartz;

namespace Openlysis.Analyzers.Contracts.Infrastructure.RateLimit;

/// <summary>
/// Provides extension methods for dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the <see cref="IRequestLimitTracker"/> service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to retrieve the configuration settings from.</param>
    /// <param name="serviceKey">The key used to identify the <see cref="IRequestLimitTracker"/> service and configured <see cref="RequestLimitOptions"/>.</param>
    /// <param name="configSectionName">The name of the parent section in the configuration.</param>
    public static void AddRequestLimitTracker(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceKey,
        string configSectionName)
    {
        // Get options.
        var limitOptions = configuration
            .GetRequiredSection(configSectionName)
            .GetRequiredSection(RequestLimitOptions.SectionName);
        ArgumentNullException.ThrowIfNull(limitOptions);

        // Add options.
        services.Configure<RequestLimitOptions>(serviceKey, limitOptions);

        // Get options monitor
        IOptionsMonitor<RequestLimitOptions> options;
        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            options = serviceProvider.GetRequiredService<IOptionsMonitor<RequestLimitOptions>>();
        }

        // Add limit tracker
        var limitTracker = new RequestLimitTracker(serviceKey, options, TimeProvider.System);
        services.AddSingleton<IRequestLimitTracker>(limitTracker);
        services.AddKeyedSingleton<IRequestLimitTracker>(serviceKey, limitTracker);
    }

    /// <summary>
    /// Adds Quartz jobs for resetting request limits on a daily and monthly basis.
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
                q.AddJob<ResetLimitJob>(jobKey);

                // Add daily trigger
                q.AddTrigger(
                    trigger =>
                    {
                        trigger
                            .ForJob(jobKey)
                            .WithIdentity("DailyResetJobTrigger")
                            .StartNow()
                            .UsingJobData(ResetLimitJob.JobDataMapKey, (int)RequestLimitPeriod.Day)
                            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0));
                    });

                // Add monthly trigger
                q.AddTrigger(
                    trigger =>
                    {
                        trigger
                            .ForJob(jobKey)
                            .WithIdentity("MonthlyResetJobTrigger")
                            .StartNow()
                            .UsingJobData(ResetLimitJob.JobDataMapKey, (int)RequestLimitPeriod.Month)
                            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0));
                    });
            });

        services.AddQuartzHostedService(
            options =>
            {
                options.WaitForJobsToComplete = false;
            });
    }
}