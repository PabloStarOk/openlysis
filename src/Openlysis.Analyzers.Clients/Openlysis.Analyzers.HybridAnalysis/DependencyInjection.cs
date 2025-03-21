using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.Contracts.Infrastructure.Client;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit;
using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Infrastructure.Services;
using Openlysis.Analyzers.HybridAnalysis.Services;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Analyzers.HybridAnalysis;

/// <summary>
/// Provides methods for registering Hybrid Analysis analyzer services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the required services for the Hybrid Analysis analyzers.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The IConfiguration instance to retrieve configuration settings from.</param>
    public static void AddHybridAnalyzer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options
        var secretOptions = configuration
            .GetRequiredSection(HybridSecretOptions.SectionName)
            .Get<HybridSecretOptions>();

        var analyzerOptionsSection = configuration
            .GetRequiredSection(HybridAnalyzerOptions.SectionName);
        var analyzerOptions = analyzerOptionsSection.Get<HybridAnalyzerOptions>();

        ArgumentNullException.ThrowIfNull(secretOptions);
        ArgumentNullException.ThrowIfNull(analyzerOptions);

        // Add options
        services.Configure<HybridAnalyzerOptions>(analyzerOptionsSection);

        // Add sandbox analyzer
        services.AddSingleton<ISandboxAnalyzer, SandboxAnalyzer>();

        // Add request limit tracker
        services.AddRequestLimitTracker(
            configuration,
            UrlAnalyzer.LimitTrackerServiceKey,
            analyzerOptions.ServiceName);

        // Add http client
        services.ConfigureHttpClient(secretOptions, analyzerOptions, client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(analyzerOptions.UserAgent);
            });

        // Add analyzer
        services.AddSingleton<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}