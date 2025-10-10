using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;

namespace Openlysis.Analyzers.Shared.Infrastructure.Logging;

/// <summary>
/// Provides extension methods for adding logging services for analyzers to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds an analyzer logger to the service collection.
    /// </summary>
    /// <typeparam name="TCategoryName">
    /// The type representing the category name for the logger. Must be non-null.
    /// </typeparam>
    /// <typeparam name="TOptions">
    /// The type of the analyzer options. Must inherit from <see cref="AnalyzerOptions"/>.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the logger will be added.</param>
    /// <param name="serviceKey">The key used to register the logger service.</param>
    public static void AddAnalyzerLogger<TCategoryName, TOptions>(
        this IServiceCollection services,
        string serviceKey)
        where TCategoryName : notnull
        where TOptions : AnalyzerOptions
    {
        services.AddKeyedSingleton<
            IServiceLogger<TCategoryName>,
            AnalyzerLogger<TCategoryName, TOptions>>(serviceKey);
    }
}