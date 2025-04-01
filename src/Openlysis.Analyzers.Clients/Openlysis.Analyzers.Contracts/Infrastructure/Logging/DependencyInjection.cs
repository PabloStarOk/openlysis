using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Core.Configuration;
using Openlysis.Analyzers.Contracts.Infrastructure.Logging.Abstractions;
using Openlysis.Analyzers.Contracts.Infrastructure.Logging.Services;

namespace Openlysis.Analyzers.Contracts.Infrastructure.Logging;

/// <summary>
/// Provides extension methods for adding logging services for analyzers to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds an analyzer logger to the service collection.
    /// </summary>
    /// <typeparam name="TOptions">The type of the analyzer options.</typeparam>
    /// <param name="services">The service collection to add the logger to.</param>
    public static void AddAnalyzerLogger<TOptions>(
        this IServiceCollection services)
        where TOptions : AnalyzerOptions
    {
        services.AddSingleton<IAnalyzerLogger, AnalyzerLogger<TOptions>>();
    }
}