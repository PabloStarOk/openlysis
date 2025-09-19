using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Doppler.NET.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Openlysis.AnalysisOrchestrator.Abstractions;
using Openlysis.AnalysisOrchestrator.Communication;
using Openlysis.AnalysisOrchestrator.Configuration;
using Openlysis.Analyzers.Filescan;
using Openlysis.Analyzers.HybridAnalysis;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Requests;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.URLQuery;
using Openlysis.Analyzers.VirusTotal;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;
using Openlysis.Infrastructure.Shared.Infrastructure.Secrets;
using Openlysis.TestTools.ServicesSimulation;

namespace Openlysis.AnalysisOrchestrator.Infrastructure;

/// <summary>
/// Provides extension methods for registering multi-analyzer worker infrastructure dependencies.
/// </summary>
internal static class DependencyInjection
{
    private const string QuartzSchedulerId = "MultiAnalyzerSchedulerId";
    private const string QuartzSchedulerName = "MultiAnalyzerScheduler";
    private const string LoggingCategory = "Openlysis.AnalysisOrchestrator";

    /// <summary>
    /// Registers the worker infrastructure services and configuration options.
    /// </summary>
    /// <param name="services">The service collection to add dependencies to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="environment">The host environment.</param>
    internal static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IUpdateMessageSender, UpdateMultiAnalysisMessageSender>();
        services.AddCommunication(configuration, environment);
        AddJobTimeoutOptions(services, configuration);
        AddDopplerServices(services, configuration);
        AddAnalyzers(services, configuration);
        AddMultiAnalyzerOptions(services, configuration);
        AddFileMultiAnalyzer(services);
        AddUrlMultiAnalyzer(services);
        AddPollingOptions(services, configuration);
        AddMultiAnalysisPoller<FileAnalysis, AnalyzeFileRequest>(services);
        AddMultiAnalysisPoller<UrlAnalysis, AnalyzeUrlRequest>(services);
    }

    private static void AddDopplerServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var dopplerOptions = configuration
            .GetRequiredSection(DopplerClientOptions.SectionName)
            .Get<DopplerClientOptions>();
        ArgumentNullException.ThrowIfNull(dopplerOptions);

        var serviceSecretOptions = configuration
            .GetRequiredSection(ServiceSecretOptions.SectionName)
            .Get<ServiceSecretOptions>();
        ArgumentNullException.ThrowIfNull(serviceSecretOptions);

        services.AddDopplerSecretsProvider(dopplerOptions, options =>
        {
            options.ApiKeySecretNames =
            [
                serviceSecretOptions.FilescanApiKeySecretName,
                serviceSecretOptions.UrlQueryApiKeySecretName,
                serviceSecretOptions.HybridAnalysisApiKeySecretName,
                serviceSecretOptions.VirusTotalApiKeySecretName,
            ];
            options.GcsCredentialSecretName = serviceSecretOptions.GcsCredentialSecretName;
        });
    }

    private static void AddJobTimeoutOptions(IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection configSection = configuration
            .GetRequiredSection(JobTimeoutOptions.SectionName);

        services.AddOptions<JobTimeoutOptions>()
            .Bind(configSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddAnalyzers(
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

        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(LoggingCategory);

        if (servicesRegistrationOptions.RegisterRealServices)
        {
            logger.LogInformation("Real analysis services registered.");
            services.AddFilescanIoAnalyzers(apiKeyOptions.FilescanApiKeySecretName, configuration);
            services.AddUrlQueryAnalyzer(apiKeyOptions.UrlQueryApiKeySecretName, configuration);
            services.AddHybridAnalyzer(apiKeyOptions.HybridAnalysisApiKeySecretName, configuration);
            services.AddVirusTotalAnalyzers(apiKeyOptions.VirusTotalApiKeySecretName, configuration);
        }

        if (servicesRegistrationOptions.RegisterSimulatedServices)
        {
            logger.LogInformation("Simulated analysis services registered.");
            services.AddSimulatedAnalysisServices(configuration);
        }

        services.AddSingleton<IReadOnlyDictionary<string, Analyzer<FileAnalysis, AnalyzeFileRequest>>>(sp =>
        {
            var analyzers = sp.GetRequiredService<IEnumerable<Analyzer<FileAnalysis, AnalyzeFileRequest>>>();
            return analyzers.ToDictionary(a => a.ServiceName);
        });

        services.AddSingleton<IReadOnlyDictionary<string, Analyzer<UrlAnalysis, AnalyzeUrlRequest>>>(sp =>
        {
            var analyzers = sp.GetRequiredService<IEnumerable<Analyzer<UrlAnalysis, AnalyzeUrlRequest>>>();
            return analyzers.ToDictionary(a => a.ServiceName);
        });
    }

    private static void AddMultiAnalyzerOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var optionsSection = configuration.GetRequiredSection(MultiAnalyzerOptions.SectionName);

        services.AddOptions<MultiAnalyzerOptions>()
            .Bind(optionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddFileMultiAnalyzer(IServiceCollection services)
    {
        AddChannel<FileAnalysis, AnalyzeFileRequest>(services);
        services.AddTransient<IMultiAnalyzer<FileAnalysis, FileAnalysisJobMessage>, FileMultiAnalyzer>();
    }

    private static void AddUrlMultiAnalyzer(IServiceCollection services)
    {
        AddChannel<UrlAnalysis, AnalyzeUrlRequest>(services);
        services.AddTransient<IMultiAnalyzer<UrlAnalysis, UrlAnalysisJobMessage>, UrlMultiAnalyzer>();
    }

    private static void AddChannel<TAnalysis, TRequest>(IServiceCollection services)
        where TAnalysis : Analysis
        where TRequest : AnalyzeRequest
    {
        services.AddTransient(
            sp =>
            {
                var analyzers = sp.GetRequiredService<
                    IReadOnlyDictionary<string, Analyzer<TAnalysis, TRequest>>>();
                var options = new BoundedChannelOptions(analyzers.Count)
                {
                    AllowSynchronousContinuations = false,
                    SingleWriter = true,
                    SingleReader = true,
                    FullMode = BoundedChannelFullMode.Wait,
                };
                return Channel.CreateBounded<TAnalysis>(options);
            });
    }

    private static void AddPollingOptions(IServiceCollection services, IConfiguration configuration)
    {
        var coordinatorOptionsSection = configuration
            .GetRequiredSection(PollingOptions.SectionName);

        services
            .AddOptions<PollingOptions>()
            .Bind(coordinatorOptionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddMultiAnalysisPoller<TAnalysis, TRequest>(IServiceCollection services)
        where TAnalysis : Analysis
        where TRequest : AnalyzeRequest
    {
        services.AddTransient(sp =>
            {
                var analyzers = sp.GetRequiredService<
                    IReadOnlyDictionary<string, Analyzer<TAnalysis, TRequest>>>();

                var options = new BoundedChannelOptions(analyzers.Count)
                {
                    AllowSynchronousContinuations = false, SingleWriter = true, SingleReader = true, FullMode = BoundedChannelFullMode.Wait,
                };
                return Channel.CreateBounded<TAnalysis[]>(options);
            });

        services.AddTransient<IMultiAnalysisPoller<TAnalysis>, MultiAnalysisPoller<TAnalysis, TRequest>>();
    }
}