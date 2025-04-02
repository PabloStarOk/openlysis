using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Shared.Core.Common.Abstractions;
using Openlysis.Analyzers.Shared.Core.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Deserialization;
using Openlysis.Analyzers.Shared.Infrastructure.Logging;
using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;
using Openlysis.Analyzers.URLQuery.Infrastructure.Services;
using Openlysis.Analyzers.URLQuery.Services;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Analyzers.URLQuery;

/// <summary>
/// Provides extension methods for adding URL query analyzer services to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the URL query analyzer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The configuration to use for the services.</param>
    public static void AddUrlQueryAnalyzer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add options
        var analyzerOptionsSection = configuration
            .GetRequiredSection(UrlQueryAnalyzerOptions.SectionName);
        var analyzerOptions = analyzerOptionsSection.Get<UrlQueryAnalyzerOptions>();

        var secretOptions = configuration
            .GetRequiredSection(UrlQuerySecretOptions.SectionName)
            .Get<UrlQuerySecretOptions>();

        var verdictCalculationOptionsSection = configuration
            .GetRequiredSection(VerdictCalculationOptions.SectionName);

        ArgumentNullException.ThrowIfNull(analyzerOptionsSection);
        ArgumentNullException.ThrowIfNull(analyzerOptions);
        ArgumentNullException.ThrowIfNull(secretOptions);
        ArgumentNullException.ThrowIfNull(verdictCalculationOptionsSection);

        services.Configure<UrlQueryAnalyzerOptions>(analyzerOptionsSection);
        services.AddOptionsWithValidateOnStart<VerdictCalculationOptions>()
            .Bind(verdictCalculationOptionsSection)
            .ValidateDataAnnotations();

        // Add analyzer logger.
        services.AddAnalyzerLogger<UrlQueryAnalyzerOptions>(
            UrlAnalyzer.KeyedServicesKey);

        // Add analyzer deserializer.
        services.AddAnalyzerDeserializer<UrlQueryAnalyzerOptions>(
            UrlAnalyzer.KeyedServicesKey,
            () => new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter<Access>(JsonNamingPolicy.CamelCase),
                    new JsonStringEnumConverter<Status>(JsonNamingPolicy.CamelCase),
                    new JsonStringEnumConverter<Severity>(JsonNamingPolicy.CamelCase),
                },
            });

        // Add http client
        services.ConfigureHttpClient(secretOptions, analyzerOptions);

        // Add URL analyzer
        services.AddSingleton<IVerdictCalculator, VerdictCalculator>();
        services.AddSingleton<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}
