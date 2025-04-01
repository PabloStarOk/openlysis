using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.API.Authentication.Infrastructure.Persistence;

namespace Openlysis.API.Authentication.Infrastructure;

/// <summary>
/// Provides extension methods for adding infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the infrastructure services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The configuration to use for setting up services.</param>
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Add auth database.
        services.AddDbContext<AuthenticationDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
}