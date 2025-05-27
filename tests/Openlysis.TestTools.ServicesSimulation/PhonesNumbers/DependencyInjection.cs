using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Domain.Phones.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Constants;
using Openlysis.TestTools.ServicesSimulation.Common.Extensions;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Reputations;
using Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Configuration;
using Openlysis.TestTools.ServicesSimulation.PhonesNumbers.Infrastructure;

namespace Openlysis.TestTools.ServicesSimulation.PhonesNumbers;

/// <summary>
/// Provides extension methods for registering simulated phone reputation services with dependency injection.
/// </summary>
internal static class DependencyInjection
{
    private const string SectionName = $"{ConfigurationSectionNames.Base}:PhoneNumberReputationServices";

    /// <summary>
    /// Registers simulated phone reputation services to the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The application configuration containing service settings.</param>
    /// <param name="logger">The logger used to record registration events.</param>
    internal static void AddSimulatedPhoneReputationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        services.AddSingleton<
            IValidateOptions<PhoneReputationStubFactoryOptions>,
            PhoneReputationStubFactoryOptionsValidator>();

        services.AddSingleton<
            IValidateOptions<ReputationServiceOptions<PhoneReputationStubFactoryOptions>>,
            ReputationServiceOptionsValidator<PhoneReputationStubFactoryOptions>>();

        services.AddSingleton<
            StubFactory<PhoneReputationStubFactoryOptions, PhoneReputation>,
            PhoneReputationStubFactory>();
        services.AddSingleton<ReputationBehaviorSimulator<
            PhoneReputation,
            PhoneReputationStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(SectionName)
            .GetChildren();

        List<string> servicesOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = DependencyInjectionExtensions.AddSimulatedServiceOptions<
                ReputationServiceOptions<PhoneReputationStubFactoryOptions>,
                PhoneReputationStubFactoryOptions>(section, services);
            servicesOptionsNames.Add(optionsName);

            services.AddSingleton<IReputationEvaluator<string, PhoneReputation>>(
                    sp => BuildSimulatedService(sp, optionsName));
        }

        DependencyInjectionExtensions.ThrowIfRepeatedNames(servicesOptionsNames);
        logger.LogInformation("Simulated phone reputation services registered.");
    }

    /// <summary>
    /// Builds a simulated phone reputation service with the specified options name.
    /// </summary>
    /// <param name="sp">The service provider used to resolve required services.</param>
    /// <param name="optionsName">The name of the options to use for this service instance.</param>
    /// <returns>A new instance of <see cref="SimulatedPhoneReputationService"/>.</returns>
    private static SimulatedPhoneReputationService BuildSimulatedService(
        IServiceProvider sp,
        string optionsName)
    {
        var logger = sp.GetRequiredService<ILogger<SimulatedPhoneReputationService>>();

        var optionsMonitor = sp.GetRequiredService<
            IOptionsMonitor<ReputationServiceOptions<PhoneReputationStubFactoryOptions>>>();

        var behaviorSimulator = sp.GetRequiredService<ReputationBehaviorSimulator<
            PhoneReputation,
            PhoneReputationStubFactoryOptions>>();

        return new SimulatedPhoneReputationService(
            logger,
            optionsName,
            optionsMonitor,
            behaviorSimulator);
    }
}