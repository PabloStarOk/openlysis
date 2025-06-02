using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Infrastructure.Communication.Consumers.Files;
using Openlysis.Infrastructure.Communication.Consumers.URLs;
using Openlysis.Infrastructure.Shared.Communication;

namespace Openlysis.Infrastructure.Communication;

/// <summary>
/// Provides extension methods for setting up the Analysis Worker dependencies required in the API.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers the consumers required for handling update analysis messages.
    /// </summary>
    /// <param name="services">The service collection to which the dependencies will be added.</param>
    /// <param name="configuration">The application configuration used for setting up dependencies.</param>
    internal static void AddUpdateAnalysisConsumers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCommunicationInfrastructure(configuration);
        services.AddMassTransit(
            x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<UpdateFileMultiAnalysisConsumer, UpdateFileMultiAnalysisConsumerDefinition>();
                x.AddConsumer<UpdateUrlMultiAnalysisConsumer, UpdateUrlMultiAnalysisConsumerDefinition>();
                x.AddRabbitMqBroker(services);
            });
    }
}