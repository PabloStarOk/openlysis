using System;
using System.Threading;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Openlysis.Analyzers.Filescan;
using Openlysis.Analyzers.HybridAnalysis;
using Openlysis.Analyzers.URLQuery;
using Openlysis.Analyzers.VirusTotal;
using Openlysis.Infrastructure.Shared.Communication;
using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota;
using Openlysis.MultiAnalyzer;
using Openlysis.MultiAnalyzer.Communication.Consumers.Files;
using Openlysis.MultiAnalyzer.Communication.Consumers.URLs;
using Openlysis.MultiAnalyzer.Infrastructure;
using Openlysis.TestTools.ServicesSimulation;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAppConfiguration(configBuilder => configBuilder.UseConfigLoader());
var shutdownTokenSource = new CancellationTokenSource();
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton(shutdownTokenSource);
    services.AddWorkerInfrastructure(context.Configuration);
    services.AddInfrastructure(context.Configuration); // TODO: Rename this method to AddCommunicationInfrastructure.
    services.AddHttpClient();

    RegisterAnalysisServices(services, context.Configuration);

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
await host.RunAsync(shutdownTokenSource.Token);
return;

static void RegisterAnalysisServices(
    IServiceCollection services,
    IConfiguration configuration)
{
    var servicesRegistrationOptions = configuration
        .GetRequiredSection(ServicesRegistrationOptions.SectionName)
        .Get<ServicesRegistrationOptions>();
    ArgumentNullException.ThrowIfNull(servicesRegistrationOptions);

    using var sp = services.BuildServiceProvider();

    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Openlysis.MultiAnalyzer));

    if (servicesRegistrationOptions.RegisterRealServices)
    {
        logger.LogInformation("Real analysis services registered.");
        services.AddFilescanIoAnalyzers(configuration);
        services.AddUrlQueryAnalyzer(configuration);
        services.AddHybridAnalyzer(configuration);
        services.AddVirusTotalAnalyzers(configuration);
    }

    if (!servicesRegistrationOptions.RegisterSimulatedServices)
    {
        return;
    }

    logger.LogInformation("Simulated analysis services registered.");
    services.AddSimulatedAnalysisServices(configuration);
}
