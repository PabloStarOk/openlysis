using System.Buffers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.URLs.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Evaluators.Ipqs;
using Openlysis.Infrastructure.Communication;
using Openlysis.Infrastructure.Configuration;
using Openlysis.Infrastructure.Persistence;
using Openlysis.Infrastructure.Persistence.Repositories;
using Openlysis.Infrastructure.Services.Files;
using Openlysis.Infrastructure.Services.Hashing;
using Openlysis.Infrastructure.Services.Messages;
using Openlysis.Infrastructure.Services.URLs;
using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;
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
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
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
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add repositories.
        services.AddScoped<IRepository<FileMultiAnalysis, GlobalId>, FileMultiAnalysisRepository>();
        services.AddScoped<IRepository<UrlMultiAnalysis, GlobalId>, UrlMultiAnalysisRepository>();
        services.AddScoped<IRepository<PhoneMultiReputation, GlobalId>, PhoneMultiReputationRepository>();
        services.AddScoped<IRepository<EmailAddressMultiReputation, GlobalId>, EmailAddressMultiReputationRepository>();
        services.AddScoped<IRepository<MessageAnalysis, GlobalId>, MessageAnalysisRepository>();

        // Add hash service.
        services.AddSingleton(ArrayPool<byte>.Shared);
        services.AddTransient<IHashService, HashService>();

        // Add multi analyzers
        services.AddScoped<IFileMultiAnalyzer, FileMultiAnalyzer>();
        services.AddScoped<IUrlMultiAnalyzer, UrlMultiAnalyzer>();

        // Add message senders
        services.AddUpdateAnalysisConsumers(configuration);
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

        // Add data detectors
        services.AddSingleton(PhoneNumberUtil.GetInstance());
        services.AddTransient<DataDetector, UrlDetector>();
        services.AddTransient<DataDetector, EmailAddressDetector>();
        services.AddTransient<DataDetector, PhoneNumberDetector>();

        // Add analysis service helpers.
        services.AddTransient<IMessageDataExtractor, MessageDataExtractor>();
        services.AddTransient<IMessageAnalyzer, MessageAnalyzer>();
        services.AddSingleton(new RecyclableMemoryStreamManager());
        services.AddTransient<IMessageAnalysisBuilder, MessageAnalysisBuilder>();

        // Add message analysis coordinator
        services.AddSingleton<IMessageAnalysisUpdater, MessageAnalysisUpdater>();
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
}