using System.Net;
using System.Text.Json;

using Filescan.Client.Abstractions;
using Filescan.Client.Constants.Common;
using Filescan.Client.Constants.Endpoints;
using Filescan.Client.Models.Common;
using Filescan.Client.Services;
using Filescan.Client.Services.Parsers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Configuration;
using Openlysis.Application.Common.Interfaces.Ports;
using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Filescan.Client;

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
    public static void AddFilescanIoAnalyzer(this IServiceCollection services, IConfiguration configuration)
    {
        // Retrieve FilescanSettings from the configuration
        var settings = configuration
            .GetRequiredSection("FilescanSettings")
            .Get<AnalyzerSettings>();

        // Ensure settings are not null
        ArgumentNullException.ThrowIfNull(settings);

        // Register model parsers
        services.AddTransient<ModelParser<FilescanError, JsonElement>, FilescanErrorParser>();
        services.AddTransient<ModelParser<ServiceFileAnalysisId, JsonElement>, AnalysisIdParser>();
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
        services.AddScoped<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>, FilescanAnalyzer>();
    }
}