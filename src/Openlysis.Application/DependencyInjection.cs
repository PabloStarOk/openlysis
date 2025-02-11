using Microsoft.Extensions.DependencyInjection;

namespace Openlysis.Application;

/// <summary>
/// Dependency injection of application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds services needed for the application layer.
    /// </summary>
    /// <param name="serviceCollection">Collection of services.</param>
    public static void AddApplication(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediatR(
            config =>
            {
                config.Lifetime = ServiceLifetime.Scoped;
                config.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);
            });
        serviceCollection.AddSingleton(TimeProvider.System);
    }
}