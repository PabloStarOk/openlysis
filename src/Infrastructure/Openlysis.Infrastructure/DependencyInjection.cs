using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Evaluators.Ipqs;
using Openlysis.Infrastructure.Persistence;
using Openlysis.Infrastructure.Persistence.Repositories;
using Openlysis.Infrastructure.Services;
using Openlysis.Infrastructure.Shared.RateQuota;

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
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Add analyses database.
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add repositories.
        services.AddScoped<IRepository<FileMultiAnalysis, FileMultiAnalysisId>, FileMultiAnalysisRepository>();
        services.AddScoped<IRepository<UrlMultiAnalysis, MultiAnalysisId>, UrlMultiAnalysisRepository>();
        services.AddScoped<IRepository<PhoneMultiReputation, Id>, PhoneMultiReputationRepository>();

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
    }
}