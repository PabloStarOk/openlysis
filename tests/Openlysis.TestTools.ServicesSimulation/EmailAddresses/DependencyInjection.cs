using System.Net.Mail;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Domain.EmailAddresses.Entities;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Constants;
using Openlysis.TestTools.ServicesSimulation.Common.Extensions;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Abstractions;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Reputations;
using Openlysis.TestTools.ServicesSimulation.EmailAddresses.Configuration;
using Openlysis.TestTools.ServicesSimulation.EmailAddresses.Infrastructure;

namespace Openlysis.TestTools.ServicesSimulation.EmailAddresses;

/// <summary>
/// Contains extension methods to register simulated email reputation services in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    private const string SectionName = $"{ConfigurationSectionNames.Base}:EmailAddressReputationServices";

    /// <summary>
    /// Registers simulated email reputation services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The configuration containing service settings.</param>
    /// <param name="logger">The logger used to record registration information.</param>
    internal static void AddSimulatedEmailReputationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        services.AddSingleton<
            IValidateOptions<EmailReputationStubFactoryOptions>,
            EmailReputationStubFactoryOptionsValidator>();

        services.AddSingleton<
            IValidateOptions<ReputationServiceOptions<EmailReputationStubFactoryOptions>>,
            ReputationServiceOptionsValidator<EmailReputationStubFactoryOptions>>();

        services.AddSingleton<
            StubFactory<EmailReputationStubFactoryOptions, EmailAddressReputation>,
            EmailReputationStubFactory>();
        services.AddSingleton<ReputationBehaviorSimulator<
            EmailAddressReputation,
            EmailReputationStubFactoryOptions>>();

        IEnumerable<IConfigurationSection> servicesSections = configuration
            .GetRequiredSection(SectionName)
            .GetChildren();

        List<string> servicesOptionsNames = [];
        foreach (var section in servicesSections)
        {
            string optionsName = DependencyInjectionExtensions.AddSimulatedServiceOptions<
                ReputationServiceOptions<EmailReputationStubFactoryOptions>,
                EmailReputationStubFactoryOptions>(section, services);
            servicesOptionsNames.Add(optionsName);

            services.AddSingleton<IReputationEvaluator<MailAddress, EmailAddressReputation>>(
                sp => BuildSimulatedService(sp, optionsName));
        }

        DependencyInjectionExtensions.ThrowIfRepeatedNames(servicesOptionsNames);
        logger.LogInformation("Simulated email reputation services registered.");
    }

    /// <summary>
    /// Builds a simulated email reputation service with the specified options.
    /// </summary>
    /// <param name="sp">The service provider to resolve dependencies.</param>
    /// <param name="optionsName">The name of the options configuration to use.</param>
    /// <returns>A configured instance of <see cref="SimulatedEmailReputationService"/>.</returns>
    private static SimulatedEmailReputationService BuildSimulatedService(
        IServiceProvider sp,
        string optionsName)
    {
        var logger = sp.GetRequiredService<ILogger<SimulatedEmailReputationService>>();

        var optionsMonitor = sp.GetRequiredService<
            IOptionsMonitor<ReputationServiceOptions<EmailReputationStubFactoryOptions>>>();

        var behaviorSimulator = sp.GetRequiredService<ReputationBehaviorSimulator<
            EmailAddressReputation,
            EmailReputationStubFactoryOptions>>();

        return new SimulatedEmailReputationService(
            logger,
            optionsName,
            optionsMonitor,
            behaviorSimulator);
    }
}