using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.Contracts.Infrastructure.Client;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit;
using Openlysis.Analyzers.Contracts.Interfaces;
using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Configuration;
using Openlysis.Analyzers.Filescan.Infrastructure.Services;
using Openlysis.Analyzers.Filescan.Services;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.URLs.Entities;

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

        // Add HTTP Client
        services.ConfigureHttpClient(secretOptions, analyzerOptions);

        // Add request limit tracker
        services.AddRequestLimitTracker(
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