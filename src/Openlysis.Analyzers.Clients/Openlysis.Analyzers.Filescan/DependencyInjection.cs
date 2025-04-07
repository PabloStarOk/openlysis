using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Configuration;
using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Analyzers.Filescan.Infrastructure.Services;
using Openlysis.Analyzers.Filescan.Services;
using Openlysis.Analyzers.Shared.Core.Common.Abstractions;
using Openlysis.Analyzers.Shared.Core.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Logging;
using Openlysis.Analyzers.Shared.Interfaces;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Deserialization;
using Openlysis.Infrastructure.Shared.RateQuota;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;

namespace Openlysis.Analyzers.Filescan;

/// <summary>
/// Provides methods for registering Filescan.IO analyzer services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Filescan.IO analyzer services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The configuration to retrieve settings from.</param>
    public static void AddFilescanIoAnalyzers(this IServiceCollection services, IConfiguration configuration)
    {
        // Get options
        var secretOptions = configuration
            .GetRequiredSection(FilescanSecretOptions.SectionName)
            .Get<FilescanSecretOptions>();

        var analyzerOptionsSection = configuration
            .GetRequiredSection(FilescanAnalyzerOptions.SectionName);
        var analyzerOptions = analyzerOptionsSection.Get<FilescanAnalyzerOptions>();

        ArgumentNullException.ThrowIfNull(secretOptions);
        ArgumentNullException.ThrowIfNull(analyzerOptions);

        // Add options
        services.Configure<FilescanAnalyzerOptions>(analyzerOptionsSection);

        // Add analyzer logger
        services.AddAnalyzerLogger<FilescanAnalyzerOptions>(
            FilescanAnalyzer.KeyedServicesKey);

        // Add analyzer deserializer.
        services.AddServiceDeserializer<FilescanAnalyzerOptions>(
            FilescanAnalyzer.KeyedServicesKey,
            () => new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter<Status>(JsonNamingPolicy.CamelCase),
                    new JsonStringEnumConverter<FilescanVerdict>(JsonNamingPolicy.SnakeCaseUpper),
                },
            });

        // Add HTTP Client
        services.ConfigureHttpClient(secretOptions, analyzerOptions);

        // Add request limit tracker
        services.AddRateQuotaService<AnalysisEndpointType>(
            configuration,
            UrlAnalyzer.LimitTrackerServiceKey,
            analyzerOptions.ServiceName);

        // Add Filescan analyzer.
        services.AddSingleton<IFilescanAnalyzer, FilescanAnalyzer>();

        // Add URL analyzer.
        services.AddSingleton<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();

        // Add file analyzer.
        services.AddSingleton<IServiceAnalyzer<ServiceFileAnalysis, ServiceAnalysisId>, FileAnalyzer>();
    }
}