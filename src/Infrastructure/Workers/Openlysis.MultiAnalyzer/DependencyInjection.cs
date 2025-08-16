using System;

using Doppler.NET.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Openlysis.Analyzers.Filescan;
using Openlysis.Analyzers.HybridAnalysis;
using Openlysis.Analyzers.URLQuery;
using Openlysis.Analyzers.VirusTotal;
using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;
using Openlysis.Infrastructure.Shared.Infrastructure.Secrets;
using Openlysis.MultiAnalyzer.Communication;
using Openlysis.MultiAnalyzer.Configuration;
using Openlysis.MultiAnalyzer.Infrastructure;
using Openlysis.TestTools.ServicesSimulation;

namespace Openlysis.MultiAnalyzer;

/// <summary>
/// Provides extension methods for registering worker-related services and analysis services
/// in the <see cref="IServiceCollection"/> for the MultiAnalyzer.
/// </summary>
internal static class DependencyInjection
{
    private const string QuartzSchedulerId = "MultiAnalyzerSchedulerId";
    private const string QuartzSchedulerName = "MultiAnalyzerScheduler";

    /// <summary>
    /// Registers all worker-related services, including infrastructure, communication consumers,
    /// analysis services, and the hosted analysis worker.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configuration">The application configuration instance.</param>
    internal static void AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddCommunicationConsumers(configuration);
        AddAnalysisServices(services, configuration);
        services.AddHostedService<AnalysisWorker>();
    }

    /// <summary>
    /// Add the analysis services (real and simulated) and related jobs based on configuration.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configuration">The application configuration instance.</param>
    private static void AddAnalysisServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var servicesRegistrationOptions = configuration
            .GetRequiredSection(ServicesRegistrationOptions.SectionName)
            .Get<ServicesRegistrationOptions>();
        ArgumentNullException.ThrowIfNull(servicesRegistrationOptions);

        var apiKeyOptions = configuration
            .GetRequiredSection(ServiceSecretOptions.SectionName)
            .Get<ServiceSecretOptions>();
        ArgumentNullException.ThrowIfNull(apiKeyOptions);

        services.AddHttpClient();
        services.AddRateQuotaRestorerJobs(QuartzSchedulerId, QuartzSchedulerName);

        using var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Openlysis.MultiAnalyzer));

        if (servicesRegistrationOptions.RegisterRealServices)
        {
            logger.LogInformation("Real analysis services registered.");
            AddDopplerServices(services, configuration);
            services.AddFilescanIoAnalyzers(apiKeyOptions.FilescanApiKeySecretName, configuration);
            services.AddUrlQueryAnalyzer(apiKeyOptions.UrlQueryApiKeySecretName, configuration);
            services.AddHybridAnalyzer(apiKeyOptions.HybridAnalysisApiKeySecretName, configuration);
            services.AddVirusTotalAnalyzers(apiKeyOptions.VirusTotalApiKeySecretName, configuration);
        }

        if (!servicesRegistrationOptions.RegisterSimulatedServices)
        {
            return;
        }

        logger.LogInformation("Simulated analysis services registered.");
        services.AddSimulatedAnalysisServices(configuration);
    }

    private static void AddDopplerServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var dopplerOptions = configuration
            .GetRequiredSection(DopplerClientOptions.SectionName)
            .Get<DopplerClientOptions>();
        ArgumentNullException.ThrowIfNull(dopplerOptions);

        var apiKeyOptions = configuration
            .GetRequiredSection(ServiceSecretOptions.SectionName)
            .Get<ServiceSecretOptions>();
        ArgumentNullException.ThrowIfNull(apiKeyOptions);

        services.AddDopplerApiKeyProvider(dopplerOptions, options =>
        {
            options.ApiKeySecretNames =
            [
                apiKeyOptions.FilescanApiKeySecretName,
                apiKeyOptions.UrlQueryApiKeySecretName,
                apiKeyOptions.HybridAnalysisApiKeySecretName,
                apiKeyOptions.VirusTotalApiKeySecretName,
            ];
        });
    }
}