using System.Text.Json;

using Filescan.Client.Abstractions;
using Filescan.Client.Models.Common;
using Filescan.Client.Models.Options;
using Filescan.Client.Services;
using Filescan.Client.Services.Parsers;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.FileAnalyses.Ports;
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
    public static void AddFilescanIoAnalyzer(this IServiceCollection services)
    {
        string? apiKey = Environment.GetEnvironmentVariable("FILESCAN_API_KEY");
        string? timeoutString = Environment.GetEnvironmentVariable("ANALYZERS_REQUEST_TIMEOUT");

        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeoutString);

        int timeout = int.Parse(timeoutString);

        services.AddTransient(_ => ApiOptions.Create(apiKey, timeout));
        services.AddTransient<ModelParser<FilescanError, JsonElement>, FilescanErrorParser>();
        services.AddTransient<ModelParser<ServiceFileAnalysisId, JsonElement>, AnalysisIdParser>();
        services.AddTransient<ModelParser<Report, JsonProperty>, ReportParser>();
        services.AddTransient<ModelParser<ServiceFileAnalysis, JsonElement>, AnalysisParser>();
        services.AddScoped<IFileScannerService, FileScanner>();
        services.AddScoped<IFileAnalyzer, FilescanAnalyzer>();
    }
}