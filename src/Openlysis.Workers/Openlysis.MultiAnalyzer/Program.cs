using Filescan.Client;
using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.MultiAnalyzer.Core;
using Openlysis.MultiAnalyzer.Features.AnalyzeFile.Consumer;
using Openlysis.MultiAnalyzer.Infrastructure;
using Openlysis.MultiAnalyzer.Infrastructure.Configuration;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    var consumerSettingsSection = context.Configuration
        .GetRequiredSection(AnalyzeFileConsumerSettings.SectionName);
    services.Configure<AnalyzeFileConsumerSettings>(consumerSettingsSection);

    services.AddInfrastructure(context.Configuration);
    services.AddFilescanIoAnalyzer(context.Configuration);
    services.AddMassTransit(
        x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<AnalyzeFileConsumer, AnalyzeFileConsumerDefinition>();
            x.AddRabbitMqBroker(services);
        });

    services.AddHostedService<AnalysisWorker>();
});

IHost host = builder.Build();
await host.RunAsync();
