using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Logging;
using Openlysis.Analyzers.URLQuery.Adapters;
using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Constants;
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;
using Openlysis.Analyzers.URLQuery.Infrastructure.Calculations;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization;

namespace Openlysis.Analyzers.URLQuery;

/// <summary>
/// Provides extension methods for adding URL query analyzer services to the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers URL query analyzer services and configuration in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="apiKeySecretName">The name of the secret containing the API key.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void AddUrlQueryAnalyzer(
        this IServiceCollection services,
        string apiKeySecretName,
        IConfiguration configuration)
    {
        // Add options
        var analyzerOptionsSection = configuration
            .GetRequiredSection(UrlQueryAnalyzerOptions.SectionName);
        var analyzerOptions = analyzerOptionsSection.Get<UrlQueryAnalyzerOptions>();

        var verdictCalculationOptionsSection = configuration
            .GetRequiredSection(VerdictCalculationOptions.SectionName);

        ArgumentNullException.ThrowIfNull(analyzerOptions);

        services.AddOptions<UrlQueryAnalyzerOptions>()
            .Bind(analyzerOptionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<VerdictCalculationOptions>()
            .Bind(verdictCalculationOptionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Add analyzer logger.
        services.AddAnalyzerLogger<UrlAnalyzer, UrlQueryAnalyzerOptions>(
            KeyedServices.GlobalKey);

        // Add analyzer deserializer.
        services.AddServiceDeserializer<UrlQueryAnalyzerOptions>(
            KeyedServices.GlobalKey,
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
        services.ConfigureHttpClient(apiKeySecretName, analyzerOptions);

        // Add URL analyzer
        services.AddSingleton<IVerdictCalculator, VerdictCalculator>();
        services.AddSingleton<Analyzer<UrlAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}
