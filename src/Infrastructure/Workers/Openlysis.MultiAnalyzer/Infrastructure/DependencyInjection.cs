using System;
using System.IO;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.MultiAnalyzer.Core.Abstractions;
using Openlysis.MultiAnalyzer.Infrastructure.Configuration;
using Openlysis.MultiAnalyzer.Infrastructure.Serialization;
using Openlysis.MultiAnalyzer.Infrastructure.Services;

namespace Openlysis.MultiAnalyzer.Infrastructure;

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

        // Local storage provider
        string tempSubDirPath = Path.Combine(Path.GetTempPath(), "openlysis");
        var dirInfo = new DirectoryInfo(tempSubDirPath);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        if (OperatingSystem.IsLinux())
        {
            dirInfo.UnixFileMode = UnixFileMode.UserExecute | UnixFileMode.UserWrite | UnixFileMode.UserRead;
        }

        services.AddSingleton(dirInfo);
        services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();

        // Endpoint uri provider
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
                    options.Converters.Add(new MultiAnalysisIdConverter());
                    options.Converters.Add(new UrlServiceAnalysisConverter());
                    return options;
                });

            cfg.ConfigureEndpoints(registrationContext);
        });
    }
}