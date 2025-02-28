using Filescan.Client;
using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.AnalysisWorker;
using Openlysis.AnalysisWorker.Configuration;
using Openlysis.AnalysisWorker.Consumer;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
    {
        var brokerSettings = context.Configuration.GetRequiredSection(BrokerSettings.SectionName).Get<BrokerSettings>();
        var consumerOptions = context.Configuration
            .GetRequiredSection(AnalysisConsumerSettings.SectionName)
            .Get<AnalysisConsumerSettings>();

        services.AddSingleton(consumerOptions);
        services.AddFilescanIoAnalyzer(context.Configuration);
        services.AddMassTransit(
            x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<FileMultiAnalysisConsumer, FileMultiAnalysisConsumerDefinition>();
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

                        cfg.ConfigureEndpoints(registrationContext);
                    });
            });

        services.AddHostedService<AnalysisWorker>();
    });

IHost host = builder.Build();
await host.RunAsync();
