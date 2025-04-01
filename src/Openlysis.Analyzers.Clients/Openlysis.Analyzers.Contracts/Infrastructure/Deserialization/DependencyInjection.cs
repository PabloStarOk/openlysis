using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Configuration;
using Openlysis.Analyzers.Contracts.Infrastructure.Deserialization.Abstractions;
using Openlysis.Analyzers.Contracts.Infrastructure.Deserialization.Services;

namespace Openlysis.Analyzers.Contracts.Infrastructure.Deserialization;

/// <summary>
/// Provides extension methods for adding deserialization services to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the analyzer deserializer service to the dependency injection container.
    /// </summary>
    /// <typeparam name="TOptions">The type of the analyzer options.</typeparam>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="serviceKey">The key to identify the service.</param>
    /// <param name="configureOptions">A function to configure the JsonSerializerOptions.</param>
    public static void AddAnalyzerDeserializer<TOptions>(
        this IServiceCollection services,
        string serviceKey,
        Func<JsonSerializerOptions> configureOptions)
        where TOptions : AnalyzerOptions
    {
        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<AnalyzerDeserializer<TOptions>>>();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();

        var deserializer = new AnalyzerDeserializer<TOptions>(logger, optionsMonitor, configureOptions());
        services.AddKeyedSingleton<IAnalyzerDeserializer>(serviceKey, deserializer);
    }
}