using Filescan.Client;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.Analyzers.HybridAnalysis;
using Openlysis.Analyzers.URLQuery;
using Openlysis.Analyzers.VirusTotal;
using Openlysis.MultiAnalyzer.Core;
using Openlysis.MultiAnalyzer.Features.AnalyzeFile.Consumer;
using Openlysis.MultiAnalyzer.Infrastructure;
using Openlysis.MultiAnalyzer.Infrastructure.Configuration;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    // Add options
    var consumerSettingsSection = context.Configuration
        .GetRequiredSection(AnalyzeFileConsumerSettings.SectionName);
    services.Configure<AnalyzeFileConsumerSettings>(consumerSettingsSection);

    services.AddInfrastructure(context.Configuration);

    // Add analyzers
    services.AddFilescanIoAnalyzer(context.Configuration);
    services.AddUrlQueryAnalyzer(context.Configuration);
    services.AddHybridAnalyzer(context.Configuration);
    services.AddVirusTotalAnalyzers(context.Configuration);

    // Add message broker
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
