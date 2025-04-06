using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Infrastructure.Services;
using Openlysis.Analyzers.HybridAnalysis.Services;
using Openlysis.Analyzers.Shared.Core.Common.Abstractions;
using Openlysis.Analyzers.Shared.Core.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Deserialization;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.RateQuota;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;

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
        services.AddRateQuotaService<AnalysisEndpointType>(
            configuration,
            UrlAnalyzer.LimitTrackerServiceKey,
            analyzerOptions.ServiceName);

        // Add analyzer logger
        services.AddSingleton<SandboxAnalyzerLogger>();

        // Add analyzer deserializer.
        services.AddAnalyzerDeserializer<HybridAnalyzerOptions>(
            SandboxAnalyzer.KeyedServicesKey,
            () => new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter<Status>(JsonNamingPolicy.SnakeCaseUpper),
                },
            });

        // Add http client
        services.ConfigureHttpClient(secretOptions, analyzerOptions, client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(analyzerOptions.UserAgent);
            });

        // Add analyzer
        services.AddSingleton<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}