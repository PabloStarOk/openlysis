using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Logging;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Analyzers.VirusTotal.Adapters;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Configuration;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Enums;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Analyzers;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Calculations;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Providers;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;

namespace Openlysis.Analyzers.VirusTotal;

/// <summary>
/// Provides methods for configuring dependency injection for VirusTotal analyzers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the VirusTotal analyzers and related services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The configuration to use for setting up the services.</param>
    public static void AddVirusTotalAnalyzers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options.
        var secretOptions = configuration
            .GetRequiredSection(VirusTotalSecretOptions.SectionName)
            .Get<VirusTotalSecretOptions>();

        var analyzerOptionsSection = configuration
            .GetRequiredSection(VirusTotalAnalyzerOptions.SectionName);
        var analyzerOptions = analyzerOptionsSection.Get<VirusTotalAnalyzerOptions>();

        var verdictCalculationOptionSection = configuration
            .GetRequiredSection(VerdictCalculationOptions.SectionName);

        ArgumentNullException.ThrowIfNull(secretOptions);
        ArgumentNullException.ThrowIfNull(analyzerOptions);

        // Add options.
        services.Configure<VirusTotalAnalyzerOptions>(analyzerOptionsSection);

        services.AddSingleton<IValidateOptions<VerdictCalculationOptions>, VerdictCalculationOptionsValidator>();
        services.AddOptions<VerdictCalculationOptions>()
            .Bind(verdictCalculationOptionSection)
            .ValidateOnStart();

        // Add VT Analyzer.
        services.AddSingleton<IVirusTotalAnalyzer, VirusTotalAnalyzer>();

        // Add analyzer logger.
        services.AddAnalyzerLogger<VirusTotalAnalyzer, VirusTotalAnalyzerOptions>(
            KeyedServices.GlobalKey);
        services.AddAnalyzerLogger<LargeFileUploadProvider, VirusTotalAnalyzerOptions>(
            KeyedServices.GlobalKey);
        services.AddAnalyzerLogger<FileAnalyzer, VirusTotalAnalyzerOptions>(
            KeyedServices.GlobalKey);
        services.AddAnalyzerLogger<UrlAnalyzer, VirusTotalAnalyzerOptions>(
            KeyedServices.GlobalKey);

        // Add analyzer deserializer.
        services.AddServiceDeserializer<VirusTotalAnalyzerOptions>(
            KeyedServices.GlobalKey,
            () => new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter<Status>(JsonNamingPolicy.KebabCaseLower),
                },
            });

        // Add http client.
        services.ConfigureHttpClient(secretOptions, analyzerOptions);

        // Add limit tracker
        services.AddRateQuotaService<AnalysisEndpointType>(
            configuration,
            KeyedServices.GlobalKey,
            analyzerOptions.ServiceName);

        // Add verdict calculator
        services.AddSingleton<IVerdictCalculator, VerdictCalculator>();

        // Add file analyzer.
        services.AddTransient<ILargeFileUploadProvider, LargeFileUploadProvider>();
        services.AddSingleton<Analyzer<FileAnalysis, AnalyzeFileRequest>, FileAnalyzer>();

        // Add URL analyzer.
        services.AddSingleton<Analyzer<UrlAnalysis, AnalyzeUrlRequest>, UrlAnalyzer>();
    }
}