using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;
using Openlysis.Infrastructure.Persistence;
using Openlysis.Infrastructure.Persistence.Repositories;
using Openlysis.Infrastructure.Services;

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
        services.AddDbContext<AnalysesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<FileMultiAnalysis, FileMultiAnalysisId>, FileMultiAnalysisRepository>();
        services.AddScoped<IFileMultiAnalysisRepository, FileMultiAnalysisRepository>();

        // Hash service
        services.AddTransient<MD5>(_ => MD5.Create());
        services.AddTransient<SHA1>(_ => SHA1.Create());
        services.AddTransient<SHA256>(_ => SHA256.Create());
        services.AddTransient<SHA512>(_ => SHA512.Create());
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IFileAnalysisService, FileAnalysisService>();
    }
}