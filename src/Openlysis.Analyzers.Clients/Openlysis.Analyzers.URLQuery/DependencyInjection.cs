using System.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Constants;
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
        services.Configure<VerdictCalculationOptions>(verdictCalculationOptionsSection);

        // Add http client
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(Addresses.Base);
        httpClient.Timeout = TimeSpan.FromMilliseconds(analyzerOptions.RequestsTimeoutMs);
        httpClient.DefaultRequestHeaders.Add(analyzerOptions.ApiKeyHeaderName, secretOptions.ApiKey);
        httpClient.DefaultRequestVersion = HttpVersion.Version20;
        services.AddKeyedSingleton(UrlAnalyzer.HttpClientServiceKey, httpClient);

        // Add URL analyzer
        services.AddSingleton<IVerdictCalculator, VerdictCalculator>();
        services.AddSingleton<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}
