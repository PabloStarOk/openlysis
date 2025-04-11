using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Files.Contracts;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.MultiAnalyzer.Adapters.Broker.Files;
using Openlysis.MultiAnalyzer.Adapters.Broker.URLs;
using Openlysis.MultiAnalyzer.Adapters.Files;
using Openlysis.MultiAnalyzer.Adapters.URLs;
using Openlysis.MultiAnalyzer.Infrastructure;

namespace Openlysis.MultiAnalyzer.Adapters;

/// <summary>
/// Provides extension methods for setting up the Analysis Worker dependencies required in the API.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Analysis Worker services and configurations to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The IConfiguration to retrieve settings from.</param>
    public static void AddAnalysisWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddMassTransit(
            x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<UpdateFileMultiAnalysisConsumer, UpdateFileMultiAnalysisConsumerDefinition>();
                x.AddConsumer<UpdateUrlMultiAnalysisConsumer, UpdateUrlMultiAnalysisConsumerDefinition>();
                x.AddRabbitMqBroker(services);
            });
        services.AddScoped<IFileMultiAnalyzer, FileMultiAnalyzer>();
        services.AddScoped<IUrlMultiAnalyzer, UrlMultiAnalyzer>();
    }
}