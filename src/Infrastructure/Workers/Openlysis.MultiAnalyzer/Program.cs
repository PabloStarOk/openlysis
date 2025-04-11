using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Openlysis.Analyzers.Filescan;
using Openlysis.Analyzers.HybridAnalysis;
using Openlysis.Analyzers.URLQuery;
using Openlysis.Analyzers.VirusTotal;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;
using Openlysis.MultiAnalyzer.Core;
using Openlysis.MultiAnalyzer.Core.Broker.Files;
using Openlysis.MultiAnalyzer.Core.Broker.URLs;
using Openlysis.MultiAnalyzer.Core.Configuration;
using Openlysis.MultiAnalyzer.Infrastructure;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    // Add options
    var consumerSettingsSection = context.Configuration
        .GetRequiredSection(AnalyzeConsumerOptions.SectionName);
    services.Configure<AnalyzeConsumerOptions>(consumerSettingsSection);

    services.AddInfrastructure(context.Configuration);

    // Add analyzers
    services.AddFilescanIoAnalyzers(context.Configuration);
    services.AddUrlQueryAnalyzer(context.Configuration);
    services.AddHybridAnalyzer(context.Configuration);
    services.AddVirusTotalAnalyzers(context.Configuration);

    // Add limit tracker jobs
    services.AddRateQuotaRestorerJobs(
        schedulerId: "MultiAnalyzerSchedulerId",
        schedulerName: "MultiAnalyzerScheduler");

    // Add message broker
    services.AddMassTransit(
        x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<AnalyzeFileConsumer, AnalyzeFileConsumerDefinition>();
            x.AddConsumer<AnalyzeUrlConsumer, AnalyzeUrlConsumerDefinition>();
            x.AddRabbitMqBroker(services);
        });

    services.AddHostedService<AnalysisWorker>();
});

IHost host = builder.Build();
await host.RunAsync();
