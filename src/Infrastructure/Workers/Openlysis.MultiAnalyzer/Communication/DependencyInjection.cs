using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.Infrastructure.Shared.Communication;
using Openlysis.MultiAnalyzer.Communication.Consumers.Files;
using Openlysis.MultiAnalyzer.Communication.Consumers.URLs;

namespace Openlysis.MultiAnalyzer.Communication;

/// <summary>
/// Provides extension methods for registering communication consumers and infrastructure
/// related to MassTransit in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers communication consumers and related infrastructure for MassTransit.
    /// </summary>
    /// <param name="services">The service collection to add consumers to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The host environment.</param>
    internal static void AddCommunicationConsumers(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddCommunicationInfrastructure(configuration, environment);
        services.AddMassTransit(
            x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<AnalyzeFileConsumer, AnalyzeFileConsumerDefinition>();
                x.AddConsumer<AnalyzeUrlConsumer, AnalyzeUrlConsumerDefinition>();
                x.AddRabbitMqBroker(services);
            });
    }
}