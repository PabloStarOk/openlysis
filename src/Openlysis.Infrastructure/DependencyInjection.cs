using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Infrastructure.Persistence;
using Openlysis.Infrastructure.Persistence.Repositories;

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

        return services;
    }
}