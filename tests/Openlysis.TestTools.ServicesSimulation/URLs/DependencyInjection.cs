using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.Client;
using Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;
using Openlysis.Domain.URLs.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Constants;
using Openlysis.TestTools.ServicesSimulation.Common.Extensions;
using Openlysis.TestTools.ServicesSimulation.Common.Services;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;
using Openlysis.TestTools.ServicesSimulation.URLs.Infrastructure;

namespace Openlysis.TestTools.ServicesSimulation.URLs;

/// <summary>
/// Provides extension methods for registering simulated URL analyzers in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    private const string SectionName = $"{ConfigurationSectionNames.Base}:UrlAnalysisServices";
    private const string KeyedServicesStubKey = "SimulatedServices";

    /// <summary>
    /// Registers simulated URL analyzers with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing analyzer settings.</param>
    /// <param name="logger">The logger for recording registration events.</param>
    internal static void AddSimulatedUrlAnalyzers(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        services.AddSingleton<
            IValidateOptions<AnalysisServiceOptions<AnalysisStubFactoryOptions>>,
            AnalysisServiceOptionsValidator<AnalysisStubFactoryOptions>>();

        services.AddSingleton<
            AnalysisStubBuilder<AnalysisStubFactoryOptions, UrlAnalysis>,
            UrlAnalysisStubBuilder>();
        services.AddSingleton<AnalysisBehaviorSimulator<UrlAnalysis, AnalysisStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(SectionName)
            .GetChildren();

        List<string> analyzersOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = DependencyInjectionExtensions
                .AddSimulatedServiceOptions<
                AnalysisServiceOptions<AnalysisStubFactoryOptions>,
                AnalysisStubFactoryOptions>(section, services);
            analyzersOptionsNames.Add(optionsName);

            services.AddSingleton<Analyzer<UrlAnalysis, AnalyzeUrlRequest>>(sp =>
            {
                var serviceOptions = sp.GetRequiredService<IOptionsMonitor<AnalysisServiceOptions<AnalysisStubFactoryOptions>>>();
                string serviceName = serviceOptions.Get(optionsName).Name;
                return BuildSimulatedUrlAnalyzer(services, sp, optionsName, serviceName);
            });
        }

        DependencyInjectionExtensions.ThrowIfRepeatedNames(analyzersOptionsNames);
        logger.LogInformation("Simulated URL analysis services registered.");
    }

    /// <summary>
    /// Builds a simulated URL analyzer with the specified configuration.
    /// </summary>
    /// <param name="services">The service collection to configure additional services.</param>
    /// <param name="sp">The service provider to resolve dependencies.</param>
    /// <param name="optionsName">The name of the options' configuration.</param>
    /// <param name="serviceName">The name of the service to simulate.</param>
    /// <returns>A configured <see cref="SimulatedUrlAnalyzer"/> instance.</returns>
    private static SimulatedUrlAnalyzer BuildSimulatedUrlAnalyzer(
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

        var rateQuotaService = DependencyInjectionExtensions.CreateRateQuotaService<
            AnalysisServiceOptions<AnalysisStubFactoryOptions>,
            AnalysisStubFactoryOptions>(sp, optionsName, serviceName);

        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var serviceLogger = loggerFactory.CreateLogger<SimulatedUrlAnalyzer>();

        var analyzerLogger = new AnalyzerLogger<SimulatedUrlAnalyzer, AnalyzerOptions>(
            serviceLogger,
            analyzerOptionsStubMonitor);

        var simulatedServiceOptions = sp.GetRequiredService<
            IOptionsMonitor<AnalysisServiceOptions<AnalysisStubFactoryOptions>>>();

        var behaviorSimulator = sp.GetRequiredService<
            AnalysisBehaviorSimulator<UrlAnalysis, AnalysisStubFactoryOptions>>();

        return new SimulatedUrlAnalyzer(
            analyzerOptionsStubMonitor,
            rateQuotaService,
            httpClientFactory,
            analyzerLogger,
            optionsName,
            simulatedServiceOptions,
            behaviorSimulator);
    }
}