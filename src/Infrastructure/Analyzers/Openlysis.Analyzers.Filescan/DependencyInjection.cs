using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Filescan.Adapters;
using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Configuration;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Analyzers.Filescan.Infrastructure.Analysis;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Logging;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;

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

        // Add analyzer loggers
        services.AddAnalyzerLogger<FilescanAnalyzer, FilescanAnalyzerOptions>(
            KeyedServices.GlobalKey);
        services.AddAnalyzerLogger<UrlAnalyzer, FilescanAnalyzerOptions>(
            KeyedServices.GlobalKey);
        services.AddAnalyzerLogger<FileAnalyzer, FilescanAnalyzerOptions>(
            KeyedServices.GlobalKey);

        // Add analyzer deserializer.
        services.AddServiceDeserializer<FilescanAnalyzerOptions>(
            KeyedServices.GlobalKey,
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
            KeyedServices.GlobalKey,
            analyzerOptions.ServiceName);

        // Add Filescan analyzer.
        services.AddSingleton<IFilescanAnalyzer, FilescanAnalyzer>();

        // Add URL analyzer.
        services.AddSingleton<Analyzer<UrlAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();

        // Add file analyzer.
        services.AddSingleton<Analyzer<FileAnalysis, AnalyzeFileRequest>, FileAnalyzer>();
    }
}