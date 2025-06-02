using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Configuration;

namespace Openlysis.MultiAnalyzer.Infrastructure.TimeoutRequests;

/// <summary>
/// Provides extension methods for registering timeout request factories in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers a <see cref="TimeoutRequestFactory{TRequest}"/> implementation in the dependency injection container,
    /// binding its options from configuration.
    /// </summary>
    /// <typeparam name="TRequest">The request type handled by the factory.</typeparam>
    /// <typeparam name="TRequestCoreFactory">
    /// The concrete factory type, which must inherit from <see cref="TimeoutRequestFactory{TRequest}"/>.
    /// </typeparam>
    /// <param name="services">The service collection to add the factory to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    internal static void AddTimeoutRequestFactory<TRequest, TRequestCoreFactory>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TRequest : class
        where TRequestCoreFactory : TimeoutRequestFactory<TRequest>
    {
        IConfigurationSection configSection = configuration
            .GetRequiredSection(TimeoutRequestOptions.SectionName);

        services.AddOptions<TimeoutRequestOptions>()
            .Bind(configSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<TimeoutRequestFactory<TRequest>, TRequestCoreFactory>();
    }
}
