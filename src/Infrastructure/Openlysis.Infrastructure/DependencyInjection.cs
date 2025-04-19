using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Evaluators.Ipqs;
using Openlysis.Infrastructure.Configuration;
using Openlysis.Infrastructure.Persistence;
using Openlysis.Infrastructure.Persistence.Repositories;
using Openlysis.Infrastructure.Services;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;

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

        var regexSettings = configuration
            .GetRequiredSection(RegexSettings.SectionName)
            .Get<RegexSettings>();

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
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
        services.AddTransient<MD5>(_ => MD5.Create());
        services.AddTransient<SHA1>(_ => SHA1.Create());
        services.AddTransient<SHA256>(_ => SHA256.Create());
        services.AddTransient<SHA512>(_ => SHA512.Create());
        services.AddScoped<IHashService, HashService>();

        // Add phone number evaluators.
        services.AddIpqsReputationEvaluators(configuration);

        // Add rate quota service jobs.
        services.AddRateQuotaRestorerJobs(
            schedulerId: "InfrastructureSchedulerId",
            schedulerName: "InfrastructureScheduler");

        // Add data detectors
        services.AddSingleton(PhoneNumberUtil.GetInstance());
        services.AddTransient<DataDetector, UrlDetector>();
        services.AddTransient<DataDetector, EmailAddressDetector>();
        services.AddTransient<DataDetector, PhoneNumberDetector>();
    }
}