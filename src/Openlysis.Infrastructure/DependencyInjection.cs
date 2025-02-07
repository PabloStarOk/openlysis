using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
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
    /// <returns>An <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AnalysesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IFileAnalysisRepository, FileAnalysisRepository>();

        // Hash service
        services.AddTransient<Encoding>(_ => Encoding.UTF8);
        services.AddKeyedSingleton<HashAlgorithm>("MD5Algorithm", (_, _) => new HMACMD5());
        services.AddKeyedSingleton<HashAlgorithm>("SHA1Algorithm", (_, _) => new HMACSHA1());
        services.AddKeyedSingleton<HashAlgorithm>("SHA256Algorithm", (_, _) => new HMACSHA256());
        services.AddKeyedSingleton<HashAlgorithm>("SHA512Algorithm", (_, _) => new HMACSHA512());
        services.AddSingleton<IHashService, HashService>();

        return services;
    }
}