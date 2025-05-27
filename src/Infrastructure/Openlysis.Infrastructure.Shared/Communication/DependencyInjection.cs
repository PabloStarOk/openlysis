using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Serialization.Common;
using Openlysis.Infrastructure.Shared.Communication.Serialization.Files;
using Openlysis.Infrastructure.Shared.Communication.Serialization.URLs;
using Openlysis.Infrastructure.Shared.Communication.Services.Broker;
using Openlysis.Infrastructure.Shared.Communication.Services.Files;

namespace Openlysis.Infrastructure.Shared.Communication;

/// <summary>
/// Provides methods for adding infrastructure services and configuring RabbitMQ broker settings.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The IConfiguration to use for configuring services.</param>
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get options
        var brokerSettingsSection = configuration
            .GetRequiredSection(BrokerSettings.SectionName);

        ArgumentNullException.ThrowIfNull(brokerSettingsSection);

        // Add options
        services.Configure<BrokerSettings>(brokerSettingsSection);

        // Add local file storage provider
        services.AddLocalFileStorageProvider(configuration);

        // Endpoint uri provider
        services.AddSingleton<IEndpointUriProvider, EndpointUriProvider>();
    }

    /// <summary>
    /// Configures RabbitMQ broker settings for MassTransit.
    /// </summary>
    /// <param name="configurator">The IBusRegistrationConfigurator to configure the message broker.</param>
    /// <param name="services">The IServiceCollection to build the service provider.</param>
    public static void AddRabbitMqBroker(
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
                    options.Converters.Add(new GlobalIdConverter());
                    options.Converters.Add(new ThreatScoreJsonConverter());
                    options.Converters.Add(new FileAnalysisJsonConverter());
                    options.Converters.Add(new FileReportJsonConverter());
                    options.Converters.Add(new UrlAnalysisJsonConverter());
                    return options;
                });

            cfg.ConfigureEndpoints(registrationContext);
        });
    }
}