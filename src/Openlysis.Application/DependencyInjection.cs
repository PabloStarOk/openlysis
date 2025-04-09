using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Phones.Interfaces;
using Openlysis.Application.Phones.Services;

namespace Openlysis.Application;

/// <summary>
/// Dependency injection of application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds services needed for the application layer.
    /// </summary>
    /// <param name="services">Collection of services.</param>
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(
            config =>
            {
                config.Lifetime = ServiceLifetime.Scoped;
                config.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);
            });
        services.AddSingleton(TimeProvider.System);

        // Add phone number reputation service
        services.AddScoped<IPhoneReputationService, PhoneReputationService>();
    }
}