using System.Net.Mail;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Domain.EmailAddresses.Entities;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Phones.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Services;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Constants;
using Openlysis.TestTools.ServicesSimulation.Common.Services;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Reputations;
using Openlysis.TestTools.ServicesSimulation.EmailAddresses.Configuration;
using Openlysis.TestTools.ServicesSimulation.EmailAddresses.Infrastructure;
using Openlysis.TestTools.ServicesSimulation.Files.Configuration;
using Openlysis.TestTools.ServicesSimulation.Files.Infrastructure;
using Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Configuration;
using Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Infrastructure;
using Openlysis.TestTools.ServicesSimulation.URLs.Infrastructure;

namespace Openlysis.TestTools.ServicesSimulation;

// TODO: Refactor this class to move each method extension methods to their respective directories.
// TODO: Add docs.

/// <summary>
/// Provides extension methods for configuring simulated analysis and reputation services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    private const string FileAnalysisServices = $"{ConfigurationSectionNames.Base}:FileAnalysisServices";
    private const string UrlAnalysisServices = $"{ConfigurationSectionNames.Base}:UrlAnalysisServices";
    private const string EmailReputationServices = $"{ConfigurationSectionNames.Base}:EmailAddressReputationServices";
    private const string PhoneReputationServices = $"{ConfigurationSectionNames.Base}:PhoneNumberReputationServices";

    /// <summary>
    /// Adds simulated analysis services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing settings for the analysis services.</param>
    public static void AddSimulatedAnalysisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<AnalysisStubFactoryOptions>,
            AnalysisStubFactoryOptionsValidator>();

        using (var sp = services.BuildServiceProvider())
        {
            if (sp.GetService<TimeProvider>() is not null)
            {
                return;
            }

            services.AddSingleton(TimeProvider.System);
        }

        AddSimulatedFileAnalyzers(services, configuration);
        AddSimulatedUrlAnalyzers(services, configuration);
    }

    /// <summary>
    /// Adds simulated reputation services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing settings for the reputation services.</param>
    public static void AddSimulatedReputationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddSimulatedEmailReputationServices(services, configuration);
        AddSimulatedPhoneReputationServices(services, configuration);
    }

    /// <summary>
    /// Adds simulated file analyzers to the service collection from configuration.
    /// </summary>
    /// <param name="services">The service collection to add the file analyzers to.</param>
    /// <param name="configuration">The configuration containing the file analyzer settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when duplicate file analyzer names are detected.</exception>
    private static void AddSimulatedFileAnalyzers(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<FileAnalysisStubFactoryOptions>,
            FileAnalysisStubFactoryOptionsValidator>();

        services.AddSingleton<
            IValidateOptions<AnalysisServiceOptions<FileAnalysisStubFactoryOptions>>,
            AnalysisServiceOptionsValidator<FileAnalysisStubFactoryOptions>>();

        services.AddSingleton<
            AnalysisStubBuilder<FileAnalysisStubFactoryOptions, FileServiceAnalysis>,
            FileAnalysisStubBuilder>();
        services.AddSingleton<AnalysisBehaviorSimulator<FileServiceAnalysis, FileAnalysisStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(FileAnalysisServices)
            .GetChildren();

        List<string> analyzersOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = AddSimulatedServiceOptions<
                AnalysisServiceOptions<FileAnalysisStubFactoryOptions>,
                FileAnalysisStubFactoryOptions>(section, services);
            analyzersOptionsNames.Add(optionsName);

            services.AddSingleton<Analyzer<FileServiceAnalysis, AnalyzeFileRequest>>(sp =>
            {
                var serviceOptions = sp.GetRequiredService<IOptionsMonitor<AnalysisServiceOptions<FileAnalysisStubFactoryOptions>>>();
                string serviceName = serviceOptions.Get(optionsName).Name;
                return BuildSimulatedFileAnalyzer(services, sp, optionsName, serviceName);
            });

            // TODO: Log registration.
        }

        if (AreRepeatedNames(analyzersOptionsNames))
        {
            throw new InvalidOperationException("Duplicate file analyzer names detected. Each analyzer must have a unique name.");
        }
    }

    private static SimulatedFileAnalyzer BuildSimulatedFileAnalyzer(
        IServiceCollection services,
        IServiceProvider sp,
        string optionsName,
        string serviceName)
    {
        var secretsOptions = new SecretOptionsStub
        {
            ApiKey = $"{serviceName}:ApiKey",
        };

        var analyzerOptionsStub = new AnalyzerOptionsStub
        {
            ApiKeyHeaderName = serviceName,
            ServiceName = serviceName,
            BaseAddress = new Uri(string.Empty, UriKind.Relative),
            RequestsTimeoutMs = 0,
        };

        services.ConfigureHttpClient(secretsOptions, analyzerOptionsStub);

        var analyzerOptionsStubMonitor =
            new SimulatedOptionsMonitor<AnalyzerOptionsStub>(analyzerOptionsStub);

        var rateQuotaService = CreateRateQuotaService<
        AnalysisServiceOptions<FileAnalysisStubFactoryOptions>,
        FileAnalysisStubFactoryOptions>(sp, optionsName, serviceName);
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var serviceLogger = new AnalyzerLogger<SimulatedFileAnalyzer, AnalyzerOptions>(
            loggerFactory.CreateLogger<SimulatedFileAnalyzer>(),
            analyzerOptionsStubMonitor);

        var simulatedServiceOptions = sp.GetRequiredService<
            IOptionsMonitor<AnalysisServiceOptions<FileAnalysisStubFactoryOptions>>>();

        var behaviorSimulator = sp.GetRequiredService<
            AnalysisBehaviorSimulator<FileServiceAnalysis, FileAnalysisStubFactoryOptions>>();

        return new SimulatedFileAnalyzer(
            analyzerOptionsStubMonitor,
            rateQuotaService,
            httpClientFactory,
            serviceLogger,
            optionsName,
            simulatedServiceOptions,
            behaviorSimulator);
    }

    /// <summary>
    /// Adds simulated URL analyzers to the service collection from configuration.
    /// </summary>
    /// <param name="services">The service collection to add the URL analyzers to.</param>
    /// <param name="configuration">The configuration containing the URL analyzer settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when duplicate URL analyzer names are detected.</exception>
    private static void AddSimulatedUrlAnalyzers(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<AnalysisServiceOptions<AnalysisStubFactoryOptions>>,
            AnalysisServiceOptionsValidator<AnalysisStubFactoryOptions>>();

        services.AddSingleton<
            AnalysisStubBuilder<AnalysisStubFactoryOptions, UrlServiceAnalysis>,
            UrlAnalysisStubBuilder>();
        services.AddSingleton<AnalysisBehaviorSimulator<UrlServiceAnalysis, AnalysisStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(UrlAnalysisServices)
            .GetChildren();

        List<string> analyzersOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = AddSimulatedServiceOptions<
                AnalysisServiceOptions<AnalysisStubFactoryOptions>,
                AnalysisStubFactoryOptions>(section, services);
            analyzersOptionsNames.Add(optionsName);

            services.AddSingleton<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>>(sp =>
            {
                var serviceOptions = sp.GetRequiredService<IOptionsMonitor<AnalysisServiceOptions<AnalysisStubFactoryOptions>>>();
                string serviceName = serviceOptions.Get(optionsName).Name;
                return BuildSimulatedUrlAnalyzer(services, sp, optionsName, serviceName);
            });

            // TODO: Log registration.
        }

        if (AreRepeatedNames(analyzersOptionsNames))
        {
            throw new InvalidOperationException("Duplicate URL analyzer names detected. Each analyzer must have a unique name.");
        }
    }

    private static SimulatedUrlAnalyzer BuildSimulatedUrlAnalyzer(
        IServiceCollection services,
        IServiceProvider sp,
        string optionsName,
        string serviceName)
    {
        var secretsOptions = new SecretOptionsStub
        {
            ApiKey = $"{serviceName}:ApiKey",
        };

        var analyzerOptionsStub = new AnalyzerOptionsStub
        {
            ApiKeyHeaderName = serviceName, ServiceName = serviceName, BaseAddress = new Uri(string.Empty, UriKind.Relative), RequestsTimeoutMs = 0,
        };

        services.ConfigureHttpClient(secretsOptions, analyzerOptionsStub);

        var analyzerOptionsStubMonitor =
            new SimulatedOptionsMonitor<AnalyzerOptionsStub>(analyzerOptionsStub);

        var rateQuotaService = CreateRateQuotaService<
            AnalysisServiceOptions<AnalysisStubFactoryOptions>,
            AnalysisStubFactoryOptions>(sp, optionsName, serviceName);
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var serviceLogger = new AnalyzerLogger<SimulatedUrlAnalyzer, AnalyzerOptions>(
            loggerFactory.CreateLogger<SimulatedUrlAnalyzer>(),
            analyzerOptionsStubMonitor);

        var simulatedServiceOptions = sp.GetRequiredService<
            IOptionsMonitor<AnalysisServiceOptions<AnalysisStubFactoryOptions>>>();

        var behaviorSimulator = sp.GetRequiredService<
            AnalysisBehaviorSimulator<UrlServiceAnalysis, AnalysisStubFactoryOptions>>();

        return new SimulatedUrlAnalyzer(
            analyzerOptionsStubMonitor,
            rateQuotaService,
            httpClientFactory,
            serviceLogger,
            optionsName,
            simulatedServiceOptions,
            behaviorSimulator);
    }

    private static RateQuotaService<AnalysisEndpointType> CreateRateQuotaService
        <TServiceOptions, TStubFactoryOptions>(
        IServiceProvider serviceProvider,
        string optionsName,
        string serviceName)
        where TServiceOptions : AnalysisServiceOptions<TStubFactoryOptions>
        where TStubFactoryOptions : AnalysisStubFactoryOptions
    {
        var rateQuotaServiceOptions = new RateQuotaServiceOptions
        {
            RateWindowRefreshIntervalMs = 1000,
        };
        var serviceOptionsMonitor =
            new SimulatedOptionsMonitor<RateQuotaServiceOptions>(rateQuotaServiceOptions);

        var analyzerOptionsMonitor = serviceProvider.GetRequiredService<
            IOptionsMonitor<TServiceOptions>>();
        var analyzerOptions = analyzerOptionsMonitor.Get(optionsName);

        const string analyzeRateQuotaOptionsName = "Analyze";
        var analyzeRateQuotaOptions = GenerateEndpointRateQuotaOptions(
            AnalysisEndpointType.Analyze,
            analyzerOptions.AnalyzeEndpoint);

        const string getStatusRateQuotaOptionsName = "GetStatus";
        var getStatusRateQuotaOptions = GenerateEndpointRateQuotaOptions(
            AnalysisEndpointType.GetStatus,
            analyzerOptions.GetStatusEndpoint);

        const string getAnalysisRateQuotaOptionsName = "GetAnalysis";
        var getAnalysisRateQuotaOptions = GenerateEndpointRateQuotaOptions(
            AnalysisEndpointType.GetResults,
            analyzerOptions.GetAnalysisEndpoint);

        IOptionsMonitor<RateQuotaEndpointOptions<AnalysisEndpointType>> endpointOptionsMonitor =
            new SimulatedOptionsMonitor<RateQuotaEndpointOptions<AnalysisEndpointType>>(
                new Dictionary<string, RateQuotaEndpointOptions<AnalysisEndpointType>>
                {
                    { analyzeRateQuotaOptionsName, analyzeRateQuotaOptions },
                    { getStatusRateQuotaOptionsName, getStatusRateQuotaOptions },
                    { getAnalysisRateQuotaOptionsName, getAnalysisRateQuotaOptions },
                });

        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        return new RateQuotaService<AnalysisEndpointType>(
            optionsInstanceName: serviceName,
            limitTrackerOptions: serviceOptionsMonitor,
            [getAnalysisRateQuotaOptionsName, getStatusRateQuotaOptionsName, analyzeRateQuotaOptionsName],
            rateQuotaOptions: endpointOptionsMonitor,
            timeProvider);
    }

    /// <summary>
    /// Generates rate quota endpoint options for a specific endpoint type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type representing endpoint types.</typeparam>
    /// <param name="endpointType">The specific endpoint type to generate options for.</param>
    /// <param name="endpointOptions">The simulated endpoint options containing rate limit settings.</param>
    /// <returns>A configured <see cref="RateQuotaEndpointOptions{TEnum}"/> instance with rate limits set.</returns>
    private static RateQuotaEndpointOptions<TEnum> GenerateEndpointRateQuotaOptions<TEnum>(
        TEnum endpointType,
        SimulatedEndpointOptions endpointOptions)
        where TEnum : Enum
    {
        return new RateQuotaEndpointOptions<TEnum>
        {
            EndpointTypes = [endpointType],
            MinuteRate = endpointOptions.MinuteRateLimit,
            HourlyRate = endpointOptions.HourRateLimit,
            DailyQuota = endpointOptions.DailyUsageLimit,
            MonthlyQuota = endpointOptions.MonthlyUsageLimit,
        };
    }

    /// <summary>
    /// Adds simulated email reputation services to the service collection from configuration.
    /// </summary>
    /// <param name="services">The service collection to add the email reputation services to.</param>
    /// <param name="configuration">The configuration containing the email reputation service settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when duplicate email reputation service names are detected.</exception>
    private static void AddSimulatedEmailReputationServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<EmailReputationStubFactoryOptions>,
            EmailReputationStubFactoryOptionsValidator>();

        services.AddSingleton<
            IValidateOptions<ReputationServiceOptions<EmailReputationStubFactoryOptions>>,
            ReputationServiceOptionsValidator<EmailReputationStubFactoryOptions>>();

        services.AddSingleton<
            StubFactory<EmailReputationStubFactoryOptions, EmailAddressServiceReputation>,
            EmailReputationStubFactory>();
        services.AddSingleton<ReputationBehaviorSimulator<
            EmailAddressServiceReputation,
            EmailReputationStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(EmailReputationServices)
            .GetChildren();

        List<string> servicesOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = AddSimulatedServiceOptions<
                ReputationServiceOptions<EmailReputationStubFactoryOptions>,
                EmailReputationStubFactoryOptions>(section, services);
            servicesOptionsNames.Add(optionsName);

            services.AddSingleton<IReputationEvaluator<MailAddress, EmailAddressServiceReputation>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<SimulatedEmailReputationService>>();

                var optionsMonitor = sp.GetRequiredService<
                    IOptionsMonitor<ReputationServiceOptions<EmailReputationStubFactoryOptions>>>();

                var behaviorSimulator = sp.GetRequiredService<ReputationBehaviorSimulator<
                    EmailAddressServiceReputation,
                    EmailReputationStubFactoryOptions>>();

                return new SimulatedEmailReputationService(
                    logger,
                    optionsName,
                    optionsMonitor,
                    behaviorSimulator);
            });
        }

        if (AreRepeatedNames(servicesOptionsNames))
        {
            throw new InvalidOperationException("Duplicate email reputation service names detected. Each analyzer must have a unique name.");
        }
    }

    /// <summary>
    /// Adds simulated phone number reputation services to the service collection from configuration.
    /// </summary>
    /// <param name="services">The service collection to add the phone reputation services to.</param>
    /// <param name="configuration">The configuration containing the phone reputation service settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when duplicate phone reputation service names are detected.</exception>
    private static void AddSimulatedPhoneReputationServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<PhoneReputationStubFactoryOptions>,
            PhoneReputationStubFactoryOptionsValidator>();

        services.AddSingleton<
            IValidateOptions<ReputationServiceOptions<PhoneReputationStubFactoryOptions>>,
            ReputationServiceOptionsValidator<PhoneReputationStubFactoryOptions>>();

        services.AddSingleton<
            StubFactory<PhoneReputationStubFactoryOptions, PhoneServiceReputation>,
            PhoneReputationStubFactory>();
        services.AddSingleton<ReputationBehaviorSimulator<
            PhoneServiceReputation,
            PhoneReputationStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(PhoneReputationServices)
            .GetChildren();

        List<string> servicesOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = AddSimulatedServiceOptions<
                ReputationServiceOptions<PhoneReputationStubFactoryOptions>,
                PhoneReputationStubFactoryOptions>(section, services);
            servicesOptionsNames.Add(optionsName);

            services.AddSingleton<IReputationEvaluator<string, PhoneServiceReputation>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<SimulatedPhoneReputationService>>();

                var optionsMonitor = sp.GetRequiredService<
                    IOptionsMonitor<ReputationServiceOptions<PhoneReputationStubFactoryOptions>>>();

                var behaviorSimulator = sp.GetRequiredService<ReputationBehaviorSimulator<
                    PhoneServiceReputation,
                    PhoneReputationStubFactoryOptions>>();

                return new SimulatedPhoneReputationService(
                    logger,
                    optionsName,
                    optionsMonitor,
                    behaviorSimulator);
            });
        }

        if (AreRepeatedNames(servicesOptionsNames))
        {
            throw new InvalidOperationException("Duplicate phone reputation service names detected. Each analyzer must have a unique name.");
        }
    }

    /// <summary>
    /// Determines whether the provided list of names contains any duplicate values.
    /// </summary>
    /// <param name="names">The list of names to check for duplicates.</param>
    /// <returns>
    /// <c>true</c> if the list contains duplicate names; otherwise, <c>false</c>.
    /// </returns>
    private static bool AreRepeatedNames(List<string> names)
    {
        List<string> distinctNames = names
            .Distinct()
            .ToList();

        return distinctNames.Count != names.Count;
    }

    /// <summary>
    /// Adds a simulated service to the service collection and configures its options from the provided configuration section.
    /// </summary>
    /// <typeparam name="TServiceOptions">The type of service options to configure.</typeparam>
    /// <typeparam name="TStubFactoryOptions">The type of stub factory options used by the service.</typeparam>
    /// <param name="section">The configuration section containing the service settings.</param>
    /// <param name="services">The service collection to add the service to.</param>
    /// <returns>The name of the configured service options.</returns>
    private static string AddSimulatedServiceOptions<TServiceOptions, TStubFactoryOptions>(
        IConfigurationSection section,
        IServiceCollection services)
        where TServiceOptions : SimulatedServiceOptions<TStubFactoryOptions>
        where TStubFactoryOptions : StubFactoryOptions
    {
        var optionsName = section
            .GetRequiredSection(nameof(SimulatedServiceOptions<TStubFactoryOptions>.Name))
            .Get<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(optionsName);

        services.AddOptions<TServiceOptions>(optionsName)
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return optionsName;
    }
}