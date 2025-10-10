using Microsoft.Extensions.Hosting;

using Openlysis.AnalysisOrchestrator;
using Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAppConfiguration(configBuilder => configBuilder.UseConfigLoader());

builder.ConfigureServices((context, services) =>
{
    services.AddWorkerServices(context.Configuration, context.HostingEnvironment);
});

IHost host = builder.Build();
await host.RunAsync();