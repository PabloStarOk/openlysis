using Filescan.Client;
using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.AnalysisWorker;
using Openlysis.AnalysisWorker.Configuration;
using Openlysis.AnalysisWorker.Consumers.AnalyzeFile;
using Openlysis.AnalysisWorker.Serialization;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    var brokerSettingsSection = context.Configuration
        .GetRequiredSection(BrokerSettings.SectionName);
    services.Configure<BrokerSettings>(brokerSettingsSection);
    var brokerSettings = brokerSettingsSection.Get<BrokerSettings>();

    var consumerSettingsSection = context.Configuration
        .GetRequiredSection(AnalyzeFileConsumerSettings.SectionName);
    services.Configure<AnalyzeFileConsumerSettings>(consumerSettingsSection);

    services.AddFilescanIoAnalyzer(context.Configuration);
    services.AddMassTransit(
        x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<AnalyzeFileConsumer, AnalyzeFileConsumerDefinition>();

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
        });

    services.AddHostedService<AnalysisWorker>();
});

IHost host = builder.Build();
await host.RunAsync();
