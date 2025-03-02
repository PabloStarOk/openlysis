using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.AnalysisWorker.Configuration;
using Openlysis.AnalysisWorker.Consumers.UpdateFileMultiAnalysis;
using Openlysis.AnalysisWorker.Serialization;
using Openlysis.AnalysisWorker.Services;
using Openlysis.Application.Common.Interfaces.Services;

namespace Openlysis.AnalysisWorker;

/// <summary>
/// Provides extension methods for setting up the Analysis Worker dependencies required in the API.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Analysis Worker services and configurations to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The IConfiguration to retrieve settings from.</param>
    public static void AddAnalysisWorker(this IServiceCollection services, IConfiguration configuration)
    {
        var brokerSettingsSection = configuration
            .GetRequiredSection(BrokerSettings.SectionName);
        services.Configure<BrokerSettings>(brokerSettingsSection);
        var brokerSettings = brokerSettingsSection.Get<BrokerSettings>();
        services.AddMassTransit(
            x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<UpdateFileMultiAnalysisConsumer, UpdateFileMultiAnalysisConsumerDefinition>();
                x.UsingRabbitMq(
                    (registrationContext, cfg) =>
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

                        cfg.ConfigureJsonSerializerOptions(options =>
                        {
                            options.Converters.Add(new FileMultiAnalysisIdJsonConverter());
                            options.Converters.Add(new ServiceFileAnalysisJsonConverter());
                            options.Converters.Add(new ReportJsonConverter());
                            return options;
                        });

                        cfg.ConfigureEndpoints(registrationContext);
                    });
            });
        services.AddScoped<IFileMultiAnalysisService, FileMultiAnalysisService>();
    }
}