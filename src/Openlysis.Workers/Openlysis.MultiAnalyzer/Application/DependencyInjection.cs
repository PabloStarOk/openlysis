using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.MultiAnalyzer.Infrastructure;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.MultiAnalyzer.Application.Services;
using Openlysis.MultiAnalyzer.Features.UpdateFileMultiAnalysis.Consumer;

namespace Openlysis.MultiAnalyzer.Application;

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
                x.AddRabbitMqBroker(services);
            });
        services.AddScoped<IFileMultiAnalysisService, FileMultiAnalysisService>();
    }
}