using System.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit;
using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
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
        var hybridSecretOptions = configuration
            .GetRequiredSection(HybridSecretOptions.SectionName)
            .Get<HybridSecretOptions>();

        var analyzerOptionsSection = configuration
            .GetRequiredSection(HybridAnalyzerOptions.SectionName);
        var analyzerOptions = analyzerOptionsSection.Get<HybridAnalyzerOptions>();

        var schedulerOptions = configuration
            .GetRequiredSection(SchedulerOptions.SectionName)
            .Get<SchedulerOptions>();

        ArgumentNullException.ThrowIfNull(hybridSecretOptions);
        ArgumentNullException.ThrowIfNull(analyzerOptions);
        ArgumentNullException.ThrowIfNull(schedulerOptions);

        // Add options
        services.Configure<HybridAnalyzerOptions>(analyzerOptionsSection);

        // Add sandbox analyzer
        services.AddSingleton<ISandboxAnalyzer, SandboxAnalyzer>();

        // Add request limit tracker
        services.AddRequestLimitTracker(
            configuration,
            analyzerOptions.ServiceName,
            schedulerOptions.Id,
            schedulerOptions.Name);

        // Add http client
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(Addresses.Base);
        httpClient.Timeout = TimeSpan.FromMilliseconds(analyzerOptions.RequestsTimeoutMs);
        httpClient.DefaultRequestHeaders.Add(analyzerOptions.ApiKeyHeaderName, hybridSecretOptions.ApiKey);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(analyzerOptions.UserAgent);
        httpClient.DefaultRequestVersion = HttpVersion.Version20;
        services.AddKeyedSingleton(UrlAnalyzer.HttpClientServiceKey, httpClient);

        // Add analyzer
        services.AddSingleton<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}