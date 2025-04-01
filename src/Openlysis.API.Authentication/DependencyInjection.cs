using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.API.Authentication.API;
using Openlysis.API.Authentication.Infrastructure;

namespace Openlysis.API.Authentication;

/// <summary>
/// Provides extension methods for adding API authentication services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds API authentication services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The configuration to use for the services.</param>
    /// <param name="environment">The web host environment.</param>
    public static void AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddInfrastructure(configuration);
        services.AddApi(environment);
    }
}