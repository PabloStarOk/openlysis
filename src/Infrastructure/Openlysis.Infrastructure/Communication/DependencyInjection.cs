using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Communication.Consumers.Common;
using Openlysis.Infrastructure.Communication.Consumers.Files;
using Openlysis.Infrastructure.Communication.Consumers.URLs;
using Openlysis.Infrastructure.Communication.Sagas.Messages;
using Openlysis.Infrastructure.Persistence;
using Openlysis.Infrastructure.Shared.Communication;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

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
    /// <param name="environment">The host environment used for configuring dependencies.</param>
    internal static void AddUpdateAnalysisConsumers(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddCommunicationInfrastructure(configuration, environment);
        services.AddMassTransit(
            x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<
                    UpdateMultiAnalysisConsumer<FileMultiAnalysis, FileAnalysis>,
                    UpdateFileMultiAnalysisConsumerDefinition>();
                x.AddConsumer<
                    UpdateMultiAnalysisConsumer<UrlMultiAnalysis, UrlAnalysis>,
                    UpdateUrlMultiAnalysisConsumerDefinition>();
                x.AddConsumer<
                    FaultedMultiAnalysisConsumer<FileAnalysis, FileAnalysisJobMessage>,
                    FailedFileAnalysesConsumerDefinition>();
                x.AddConsumer<
                    FaultedMultiAnalysisConsumer<UrlAnalysis, UrlAnalysisJobMessage>,
                    FailedUrlAnalysesConsumerDefinition>();
                x.AddSagaStateMachine<
                    MessageAnalysisUpdateStateMachine,
                    MessageAnalysisUpdateSaga,
                    MessageAnalysisUpdateDefinition>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<ApplicationDbContext>();
                        r.UsePostgres();
                    });
                x.AddRabbitMqBroker(services);
            });
    }
}