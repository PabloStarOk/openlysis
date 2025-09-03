using Microsoft.Extensions.Hosting;

using Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader;
using Openlysis.MultiAnalyzer;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAppConfiguration(configBuilder => configBuilder.UseConfigLoader());

builder.ConfigureServices((context, services) =>
{
    services.AddWorkerServices(context.Configuration, context.HostingEnvironment);
});

IHost host = builder.Build();
await host.RunAsync();