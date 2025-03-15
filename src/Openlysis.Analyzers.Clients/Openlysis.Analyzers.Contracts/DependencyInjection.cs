using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Configuration;
using Openlysis.Analyzers.Contracts.Enums;
using Openlysis.Analyzers.Contracts.Interfaces;
using Openlysis.Analyzers.Contracts.Services;

using Quartz;

namespace Openlysis.Analyzers.Contracts;

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
    /// <param name="parentSectionName">The name of the parent section in the configuration.</param>
    /// <param name="schedulerId">The identifier for the Quartz scheduler.</param>
    /// <param name="schedulerName">The name for the Quartz scheduler.</param>
    public static void AddRequestLimitManager(
        this IServiceCollection services,
        IConfiguration configuration,
        string parentSectionName,
        string schedulerId,
        string schedulerName)
    {
        // Add options.
        var limitOptions = configuration
            .GetRequiredSection(parentSectionName)
            .GetRequiredSection(RequestLimitOptions.SectionName);
        ArgumentNullException.ThrowIfNull(limitOptions);
        services.Configure<RequestLimitOptions>(limitOptions);
        
        // Add limit manager.
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IRequestLimitTracker, RequestLimitTracker>();

        using var sp = services.BuildServiceProvider();
        
        // Add quartz.
        services.AddQuartz(
            q =>
            {
                q.SchedulerId = schedulerId;
                q.SchedulerName = schedulerName;
                var dailyJobKey = new JobKey("DailyResetJob");
                q.AddJob<ResetLimitJob>(dailyJobKey);
                q.AddTrigger(
                    trigger =>
                    {
                        trigger
                            .ForJob(dailyJobKey)
                            .WithIdentity("DailyResetJobTrigger")
                            .StartNow()
                            .UsingJobData(ResetLimitJob.JobDataMapKey, (int)RequestLimitPeriod.Day)
                            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0));
                    });
                
                var monthlyJobKey = new JobKey("MonthlyResetJob");
                q.AddJob<ResetLimitJob>(monthlyJobKey);
                q.AddTrigger(
                    trigger =>
                    {
                        trigger
                            .ForJob(monthlyJobKey)
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