using System;

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Openlysis.Analyzers.Filescan;
using Openlysis.Analyzers.HybridAnalysis;
using Openlysis.Analyzers.URLQuery;
using Openlysis.Analyzers.VirusTotal;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;
using Openlysis.Infrastructure.Shared.Messaging;
using Openlysis.MultiAnalyzer;
using Openlysis.MultiAnalyzer.Configuration;
using Openlysis.MultiAnalyzer.Consumers.Files;
using Openlysis.MultiAnalyzer.Consumers.URLs;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((context, services) =>
{
    // Get options
    var consumerSettingsSection = context.Configuration
        .GetRequiredSection(AnalyzeConsumerOptions.SectionName);

    ArgumentNullException.ThrowIfNull(consumerSettingsSection);

    // Add options
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
