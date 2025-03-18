using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Configuration;
using Openlysis.Analyzers.Contracts.Interfaces;
using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Constants.Common;
using Openlysis.Analyzers.Filescan.Core.Constants.Endpoints;
using Openlysis.Analyzers.Filescan.Core.Models.Common;
using Openlysis.Analyzers.Filescan.Infrastructure.Services;
using Openlysis.Analyzers.Filescan.Infrastructure.Services.Parsers;
using Openlysis.Analyzers.Filescan.Services;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.FileAnalyses.Entities;

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
        // Retrieve FilescanSettings from the configuration
        var settings = configuration
            .GetRequiredSection("FilescanSettings")
            .Get<AnalyzerSettings>();

        // Ensure settings are not null
        ArgumentNullException.ThrowIfNull(settings);

        // Register model parsers
        services.AddTransient<ModelParser<FilescanError, JsonElement>, FilescanErrorParser>();
        services.AddTransient<ModelParser<ServiceAnalysisId, JsonElement>, AnalysisIdParser>();
        services.AddTransient<ModelParser<Report, JsonProperty>, ReportParser>();
        services.AddTransient<ModelParser<ServiceFileAnalysis, JsonElement>, AnalysisParser>();

        // Configure HttpClient for Filescan.IO service
        services.AddHttpClient(ServiceConstants.ServiceName, httpClient =>
        {
            httpClient.DefaultRequestVersion = HttpVersion.Version30;
            httpClient.Timeout = TimeSpan.FromMilliseconds(settings.RequestsTimeoutMs);
            httpClient.BaseAddress = new Uri(Addresses.BaseAddress);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.ApiKey, settings.ApiKey);
        });

        // Register Filescan.IO services
        services.AddScoped<IFileScannerService, FileScanner>();
        services.AddScoped<IServiceAnalyzer<ServiceFileAnalysis, ServiceAnalysisId>, FileAnalyzer>();
    }
}