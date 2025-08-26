using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader;
using Openlysis.MultiAnalyzer;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureAppConfiguration(configBuilder => configBuilder.UseConfigLoader());

var shutdownTokenSource = new CancellationTokenSource();
builder.ConfigureServices((context, services) =>
{
    services.AddSingleton(shutdownTokenSource);
    services.AddWorkerServices(context.Configuration, context.HostingEnvironment);
});

IHost host = builder.Build();
await host.RunAsync(shutdownTokenSource.Token);