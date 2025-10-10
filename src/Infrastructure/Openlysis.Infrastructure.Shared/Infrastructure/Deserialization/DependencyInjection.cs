using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Services;

namespace Openlysis.Infrastructure.Shared.Infrastructure.Deserialization;

/// <summary>
/// Provides extension methods for adding deserialization services to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the deserializer service to the dependency injection container.
    /// </summary>
    /// <typeparam name="TOptions">An implementation of <see cref="ServiceOptions"/>.</typeparam>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="serviceKey">The key to identify the service.</param>
    /// <param name="configureOptions">A function to configure the JsonSerializerOptions.</param>
    public static void AddServiceDeserializer<TOptions>(
        this IServiceCollection services,
        string serviceKey,
        Func<JsonSerializerOptions> configureOptions)
        where TOptions : ServiceOptions
    {
        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ServiceDeserializer<TOptions>>>();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();

        var deserializer = new ServiceDeserializer<TOptions>(logger, optionsMonitor, configureOptions());
        services.AddKeyedSingleton<IServiceDeserializer>(serviceKey, deserializer);
    }
}