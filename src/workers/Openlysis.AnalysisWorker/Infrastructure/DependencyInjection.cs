using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.AnalysisWorker.Core.Abstractions;
using Openlysis.AnalysisWorker.Infrastructure.Configuration;
using Openlysis.AnalysisWorker.Infrastructure.Serialization;
using Openlysis.AnalysisWorker.Infrastructure.Services;

namespace Openlysis.AnalysisWorker.Infrastructure;

/// <summary>
/// Provides methods for adding infrastructure services and configuring RabbitMQ broker settings.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The IConfiguration to use for configuring services.</param>
    internal static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var brokerSettingsSection = configuration
            .GetRequiredSection(BrokerSettings.SectionName);
        services.Configure<BrokerSettings>(brokerSettingsSection);

        services.AddSingleton<IEndpointUriProvider, EndpointUriProvider>();
    }

    /// <summary>
    /// Configures RabbitMQ broker settings for MassTransit.
    /// </summary>
    /// <param name="configurator">The IBusRegistrationConfigurator to configure the message broker.</param>
    /// <param name="services">The IServiceCollection to build the service provider.</param>
    internal static void AddRabbitMqBroker(
        this IBusRegistrationConfigurator configurator,
        IServiceCollection services)
    {
        BrokerSettings brokerSettings;
        using (var serviceProvider = services.BuildServiceProvider())
        {
            brokerSettings = serviceProvider
                .GetRequiredService<IOptions<BrokerSettings>>()
                .Value;
        }

        configurator.UsingRabbitMq((registrationContext, cfg) =>
        {
            cfg.Host(
                brokerSettings.Host,
                brokerSettings.Port,
                brokerSettings.VirtualHost,
                hostConfig =>
                {
                    hostConfig.Username(brokerSettings.Username);
                    hostConfig.Password(brokerSettings.Password);
                });

            cfg.ConfigureJsonSerializerOptions(
                options =>
                {
                    options.Converters.Add(new FileMultiAnalysisIdJsonConverter());
                    options.Converters.Add(new ServiceFileAnalysisJsonConverter());
                    options.Converters.Add(new ReportJsonConverter());
                    return options;
                });

            cfg.ConfigureEndpoints(registrationContext);
        });
    }
}