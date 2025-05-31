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

        ArgumentNullException.ThrowIfNull(analyzerOptions);
        ArgumentNullException.ThrowIfNull(secretOptions);

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
        services.ConfigureHttpClient(secretOptions, analyzerOptions);

        // Add URL analyzer
        services.AddSingleton<IVerdictCalculator, VerdictCalculator>();
        services.AddSingleton<Analyzer<UrlAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}
