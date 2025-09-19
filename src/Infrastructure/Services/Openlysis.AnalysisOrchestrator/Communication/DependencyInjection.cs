using System;
using System.Data;
using System.Reflection;

using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Openlysis.AnalysisOrchestrator.Communication.Consumers.Common;
using Openlysis.AnalysisOrchestrator.Communication.Consumers.Files;
using Openlysis.AnalysisOrchestrator.Communication.Consumers.URLs;
using Openlysis.AnalysisOrchestrator.Communication.Serialization;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.AnalysisOrchestrator.Communication;

/// <summary>
/// Provides extension methods for registering communication consumers and infrastructure
/// related to MassTransit in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    private const string DatabaseConnectionString = "MassTransit";
    private const string MigrationsHistoryTable = "job_service_saga_db_context";

    /// <summary>
    /// Registers communication services and related infrastructure for MassTransit.
    /// </summary>
    /// <param name="services">The service collection to add consumers to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The host environment.</param>
    internal static void AddCommunication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddCommunicationInfrastructure(configuration, environment);

        services.AddDbContext<JobServiceSagaDbContext>(builder =>
        {
            builder.UseNpgsql(
                configuration.GetConnectionString(DatabaseConnectionString),
                npgsqlBuilder =>
                {
                    npgsqlBuilder.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    npgsqlBuilder.MigrationsHistoryTable(MigrationsHistoryTable);
                    npgsqlBuilder.EnableRetryOnFailure();
                });
            builder.UseSnakeCaseNamingConvention();
        });

        services.AddMassTransit(
            x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<AnalysisJobConsumer<FileAnalysis, FileAnalysisJobMessage>, FileAnalysisJobConsumerDefinition>();
                x.AddConsumer<AnalysisJobConsumer<UrlAnalysis, UrlAnalysisJobMessage>, UrlAnalysisJobConsumerDefinition>();
                x.AddDelayedMessageScheduler();
                x.AddJobSagaStateMachines(cfg => cfg.FinalizeCompleted = true)
                    .EntityFrameworkRepository(configurator =>
                    {
                        configurator.ConcurrencyMode = ConcurrencyMode.Optimistic;
                        configurator.IsolationLevel = IsolationLevel.ReadCommitted;
                        configurator.LockStatementProvider = new PostgresLockStatementProvider();
                        configurator.UsePostgres();
                        configurator.ExistingDbContext<JobServiceSagaDbContext>();
                    });

                x.AddRabbitMqBroker(services, (_, configurator) =>
                {
                    configurator.UseDelayedMessageScheduler();
                    configurator.ConfigureJsonSerializerOptions(options =>
                    {
                        options.Converters.Add(new ExternalAnalysisIdJsonConverter());
                        return options;
                    });
                });
            });

        services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromMinutes(1);
                options.StopTimeout = TimeSpan.FromMinutes(1);
            });
    }
}