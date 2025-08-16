using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;
using Openlysis.Domain.Files.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Constants;
using Openlysis.TestTools.ServicesSimulation.Common.Extensions;
using Openlysis.TestTools.ServicesSimulation.Common.Services;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;
using Openlysis.TestTools.ServicesSimulation.Files.Configuration;
using Openlysis.TestTools.ServicesSimulation.Files.Infrastructure;

namespace Openlysis.TestTools.ServicesSimulation.Files;

/// <summary>
/// Provides extension methods for registering simulated file analyzers in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    private const string SectionName = $"{ConfigurationSectionNames.Base}:FileAnalysisServices";
    private const string KeyedServicesStubKey = "SimulatedServices";

    /// <summary>
    /// Registers simulated file analyzer services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The application configuration containing analyzer settings.</param>
    /// <param name="logger">The logger instance for recording registration activities.</param>
    internal static void AddSimulatedFileAnalyzers(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        services.AddSingleton<
            IValidateOptions<FileAnalysisStubFactoryOptions>,
            FileAnalysisStubFactoryOptionsValidator>();

        services.AddSingleton<
            IValidateOptions<AnalysisServiceOptions<FileAnalysisStubFactoryOptions>>,
            AnalysisServiceOptionsValidator<FileAnalysisStubFactoryOptions>>();

        services.AddSingleton<
            AnalysisStubBuilder<FileAnalysisStubFactoryOptions, FileAnalysis>,
            FileAnalysisStubBuilder>();
        services.AddSingleton<AnalysisBehaviorSimulator<FileAnalysis, FileAnalysisStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(SectionName)
            .GetChildren();

        List<string> analyzersOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = DependencyInjectionExtensions
                .AddSimulatedServiceOptions<
                AnalysisServiceOptions<FileAnalysisStubFactoryOptions>,
                FileAnalysisStubFactoryOptions>(section, services);
            analyzersOptionsNames.Add(optionsName);

            services.AddSingleton<Analyzer<FileAnalysis, AnalyzeFileRequest>>(sp =>
            {
                var serviceOptions = sp.GetRequiredService<IOptionsMonitor<AnalysisServiceOptions<FileAnalysisStubFactoryOptions>>>();
                string serviceName = serviceOptions.Get(optionsName).Name;
                return BuildSimulatedFileAnalyzer(services, sp, optionsName, serviceName);
            });
        }

        DependencyInjectionExtensions.ThrowIfRepeatedNames(analyzersOptionsNames);
        logger.LogInformation("Simulated file analysis services registered.");
    }

    /// <summary>
    /// Builds a simulated file analyzer with the specified options and service configuration.
    /// </summary>
    /// <param name="services">The service collection to register additional dependencies.</param>
    /// <param name="sp">The service provider to resolve dependencies from.</param>
    /// <param name="optionsName">The name of the options to use for this analyzer instance.</param>
    /// <param name="serviceName">The name of the service being simulated.</param>
    /// <returns>A configured instance of <see cref="SimulatedFileAnalyzer"/>.</returns>
    private static SimulatedFileAnalyzer BuildSimulatedFileAnalyzer(
        IServiceCollection services,
        IServiceProvider sp,
        string optionsName,
        string serviceName)
    {
        var analyzerOptionsStub = DependencyInjectionExtensions
            .GenerateAnalyzerOptionsStub(serviceName);

        services.ConfigureHttpClient(KeyedServicesStubKey, analyzerOptionsStub);

        var analyzerOptionsStubMonitor =
            new SimulatedOptionsMonitor<AnalyzerOptions>(analyzerOptionsStub);

        var rateQuotaService = DependencyInjectionExtensions
            .CreateRateQuotaService<
                AnalysisServiceOptions<FileAnalysisStubFactoryOptions>,
                FileAnalysisStubFactoryOptions>(sp, optionsName, serviceName);

        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var serviceLogger = new AnalyzerLogger<SimulatedFileAnalyzer, AnalyzerOptions>(
            loggerFactory.CreateLogger<SimulatedFileAnalyzer>(),
            analyzerOptionsStubMonitor);

        var simulatedServiceOptions = sp.GetRequiredService<
            IOptionsMonitor<AnalysisServiceOptions<FileAnalysisStubFactoryOptions>>>();

        var behaviorSimulator = sp.GetRequiredService<
            AnalysisBehaviorSimulator<FileAnalysis, FileAnalysisStubFactoryOptions>>();

        return new SimulatedFileAnalyzer(
            analyzerOptionsStubMonitor,
            rateQuotaService,
            httpClientFactory,
            serviceLogger,
            optionsName,
            simulatedServiceOptions,
            behaviorSimulator);
    }
}