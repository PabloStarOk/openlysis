using System.Buffers;
using System.Text;

using Doppler.NET.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.URLs.Contracts.Abstractions;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Evaluators.Ipqs;
using Openlysis.Infrastructure.Communication;
using Openlysis.Infrastructure.Configuration;
using Openlysis.Infrastructure.Persistence;
using Openlysis.Infrastructure.Persistence.Repositories;
using Openlysis.Infrastructure.Services.Files;
using Openlysis.Infrastructure.Services.Finders;
using Openlysis.Infrastructure.Services.Hashing;
using Openlysis.Infrastructure.Services.Messages;
using Openlysis.Infrastructure.Services.URLs;
using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.PipePool;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;
using Openlysis.Infrastructure.Shared.Infrastructure.Secrets;
using Openlysis.TestTools.ServicesSimulation;

using PhoneNumbers;

namespace Openlysis.Infrastructure;

/// <summary>
/// Dependency injection of infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add infrastructure implementations.
    /// </summary>
    /// <param name="services">Collection of services.</param>
    /// <param name="configuration">Configuration of the application.</param>
    /// <param name="environment">Host environment information.</param>
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Get options
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var regexSettings = configuration
            .GetRequiredSection(RegexSettings.SectionName)
            .Get<RegexSettings>();
        ArgumentNullException.ThrowIfNull(regexSettings);

        // Add options
        AppDomain.CurrentDomain.SetData(
            "REGEX_DEFAULT_MATCH_TIMEOUT",
            TimeSpan.FromMilliseconds(regexSettings.TimeoutMs));

        // Add analyses database.
        services.AddDbContext<DbContext, ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add repositories.
        services.AddScoped<IRepository<FileMultiAnalysis>, Repository<FileMultiAnalysis>>();
        services.AddScoped<IRepository<UrlMultiAnalysis>, Repository<UrlMultiAnalysis>>();
        services.AddScoped<IRepository<PhoneMultiReputation>, Repository<PhoneMultiReputation>>();
        services.AddScoped<IRepository<EmailAddressMultiReputation>, Repository<EmailAddressMultiReputation>>();
        services.AddScoped<IRepository<MessageAnalysis>, Repository<MessageAnalysis>>();

        services.AddSingleton(_ => MemoryPool<byte>.Shared);

        // Add hash serviceS.
        services.AddTransient<IHashService, HashService>();
        services.AddSingleton(Encoding.UTF8);
        services.AddTransient<IMessageHashService, MessageHashService>();

        // Add multi analyzers
        services.AddScoped<IFileMultiAnalysisQueue, FileMultiAnalysisQueue>();
        services.AddScoped<IUrlMultiAnalysisQueue, UrlMultiAnalysisQueue>();

        AddDopplerServices(services, configuration);

        // Add message senders
        services.AddUpdateAnalysisConsumers(configuration, environment);
        using (var sp = services.BuildServiceProvider())
        {
            var logger = sp
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(Infrastructure));
            RegisterReputationServices(services, configuration, logger);
        }

        // Add rate quota service jobs.
        services.AddRateQuotaRestorerJobs(
            schedulerId: "InfrastructureSchedulerId",
            schedulerName: "InfrastructureScheduler");

        // Add file processor
        AddFileStorageContext(services, configuration);

        // Add data detectors
        services.AddSingleton(PhoneNumberUtil.GetInstance());
        services.AddTransient<DataDetector, UrlDetector>();
        services.AddTransient<DataDetector, EmailAddressDetector>();
        services.AddTransient<DataDetector, PhoneNumberDetector>();

        // Add analysis service helpers.
        services.AddTransient<IMessageDataExtractor, MessageDataExtractor>();
        services.AddTransient<IMessageAnalyzer, MessageAnalyzer>();
        services.AddSingleton(new RecyclableMemoryStreamManager());

        services.AddTransient<IMessageAnalysisQueue, MessageAnalysisQueue>();
        AddRecentAnalysisFinders(services, configuration);
    }

    /// <summary>
    /// Registers reputation services based on application configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration containing service registration options.</param>
    /// <param name="logger">The logger used to log registration information.</param>
    private static void RegisterReputationServices(
        IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        var servicesRegistrationOptions = configuration
            .GetRequiredSection(ServicesRegistrationOptions.SectionName)
            .Get<ServicesRegistrationOptions>();
        ArgumentNullException.ThrowIfNull(servicesRegistrationOptions);

        if (servicesRegistrationOptions.RegisterRealServices)
        {
            logger.LogInformation("Real analysis services registered.");
            services.AddIpqsReputationEvaluators(configuration);
        }

        if (!servicesRegistrationOptions.RegisterSimulatedServices)
        {
            return;
        }

        logger.LogInformation("Simulated analysis services registered.");
        services.AddSimulatedReputationServices(configuration);
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
            options.ApiKeySecretNames = [serviceSecretOptions.IpqsApiKeySecretName];
            options.GcsCredentialSecretName = serviceSecretOptions.GcsCredentialSecretName;
        });
    }

    private static void AddFileStorageContext(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var optionsSection = configuration.GetSection(FileStorageContextOptions.SectionName);

        services.AddOptions<FileStorageContextOptions>()
            .Bind(optionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddPipePool();
        services.AddScoped<IFileStorageContext, FileStorageContext>();
    }

    private static void AddRecentAnalysisFinders(IServiceCollection services, IConfiguration configuration)
    {
        var optionsSection = configuration.GetRequiredSection(AnalysisReuseOptions.SectionName);

        services.AddOptions<AnalysisReuseOptions>()
            .Bind(optionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<
            IRecentAnalysisFinder<FileMultiAnalysis>,
            RecentMultiAnalysisFinder<FileAnalysis, FileMultiAnalysis>>();

        services.AddTransient<
            IRecentAnalysisFinder<UrlMultiAnalysis>,
            RecentMultiAnalysisFinder<UrlAnalysis, UrlMultiAnalysis>>();

        services.AddTransient<
            IRecentAnalysisFinder<MessageAnalysis>,
            RecentMessageAnalysisFinder>();
    }
}