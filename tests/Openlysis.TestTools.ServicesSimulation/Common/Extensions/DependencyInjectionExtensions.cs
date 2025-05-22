using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Services;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Services;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

namespace Openlysis.TestTools.ServicesSimulation.Common.Extensions;

/// <summary>
/// Provides extension methods for service collection to register simulated services.
/// </summary>
internal static class DependencyInjectionExtensions
{
    /// <summary>
    /// Generates stub options for secret configuration with a predefined API key format.
    /// </summary>
    /// <param name="serviceName">The name of the service to generate API key for.</param>
    /// <returns>A <see cref="SecretOptions"/> instance with the generated API key.</returns>
    internal static SecretOptions GenerateSecretStubOptions(string serviceName)
    {
        return new SecretOptionsStub
        {
            ApiKey = $"{serviceName}:ApiKey",
        };
    }

    /// <summary>
    /// Generates stub options for analyzer configuration with predefined values.
    /// </summary>
    /// <param name="serviceName">The name of the service to generate configuration for.</param>
    /// <returns>A <see cref="AnalyzerOptions"/> instance with the generated configuration.</returns>
    internal static AnalyzerOptions GenerateAnalyzerOptionsStub(string serviceName)
    {
        return new AnalyzerOptionsStub
        {
            ApiKeyHeaderName = serviceName,
            ServiceName = serviceName,
            BaseAddress = new Uri(string.Empty, UriKind.Relative),
            RequestsTimeoutMs = 0,
        };
    }

    /// <summary>
    /// Creates a rate quota service for a specific analysis service.
    /// </summary>
    /// <typeparam name="TServiceOptions">The type of service options that inherits from AnalysisServiceOptions.</typeparam>
    /// <typeparam name="TStubFactoryOptions">The type of stub factory options that inherits from AnalysisStubFactoryOptions.</typeparam>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <param name="optionsName">The name of the options configuration to retrieve.</param>
    /// <param name="serviceName">The name of the service used for identification.</param>
    /// <returns>A configured <see cref="RateQuotaService{AnalysisEndpointType}"/> instance.</returns>
    internal static RateQuotaService<AnalysisEndpointType> CreateRateQuotaService
        <TServiceOptions, TStubFactoryOptions>(
        IServiceProvider serviceProvider,
        string optionsName,
        string serviceName)
        where TServiceOptions : AnalysisServiceOptions<TStubFactoryOptions>
        where TStubFactoryOptions : AnalysisStubFactoryOptions
    {
        var rateQuotaServiceOptions = new RateQuotaServiceOptions
        {
            RateWindowRefreshIntervalMs = 1000,
        };

        var serviceOptionsMonitor =
            new SimulatedOptionsMonitor<RateQuotaServiceOptions>(rateQuotaServiceOptions);

        var simulatedServiceOptions = serviceProvider.GetRequiredService<
            IOptionsMonitor<TServiceOptions>>();

        var rateQuotaOptionsMap = new Dictionary<string, AnalysisEndpointType>()
        {
            { "Analyze", AnalysisEndpointType.Analyze },
            { "GetStatus", AnalysisEndpointType.GetStatus },
            { "GetAnalysis", AnalysisEndpointType.GetResults },
        };
        var endpointOptionsMonitor = new RateQuotaOptionsMonitor<TStubFactoryOptions>(
            optionsName,
            simulatedServiceOptions,
            rateQuotaOptionsMap);

        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        return new RateQuotaService<AnalysisEndpointType>(
            optionsInstanceName: serviceName,
            limitTrackerOptions: serviceOptionsMonitor,
            rateQuotaOptionKeys: rateQuotaOptionsMap.Keys.ToArray(),
            rateQuotaOptions: endpointOptionsMonitor,
            timeProvider);
    }

    /// <summary>
    /// Validates that the provided list of names contains no duplicates.
    /// </summary>
    /// <param name="names">The list of names to check for duplicates.</param>
    internal static void ThrowIfRepeatedNames(List<string> names)
    {
        List<string> distinctNames = names
            .Distinct()
            .ToList();

        if (distinctNames.Count != names.Count)
        {
            throw new InvalidOperationException("Duplicate names detected for simulated services. Each analyzer must have a unique name.");
        }
    }

    /// <summary>
    /// Registers a simulated service with its configuration options in the dependency injection container.
    /// </summary>
    /// <typeparam name="TServiceOptions">The type of service options to register.</typeparam>
    /// <typeparam name="TStubFactoryOptions">The type of stub factory options used by the service.</typeparam>
    /// <param name="section">The configuration section containing the service options.</param>
    /// <param name="services">The service collection to add the options to.</param>
    /// <returns>The name of the registered options.</returns>
    /// <exception cref="ArgumentException">Thrown when the options name is null or whitespace.</exception>
    internal static string AddSimulatedServiceOptions<TServiceOptions, TStubFactoryOptions>(
        IConfigurationSection section,
        IServiceCollection services)
        where TServiceOptions : SimulatedServiceOptions<TStubFactoryOptions>
        where TStubFactoryOptions : StubFactoryOptions
    {
        var optionsName = section
            .GetRequiredSection(nameof(SimulatedServiceOptions<TStubFactoryOptions>.Name))
            .Get<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(optionsName);

        services.AddOptions<TServiceOptions>(optionsName)
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return optionsName;
    }
}