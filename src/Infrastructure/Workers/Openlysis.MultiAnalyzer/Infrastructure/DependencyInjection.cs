using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Communication;
using Openlysis.MultiAnalyzer.Configuration;
using Openlysis.MultiAnalyzer.Infrastructure.Communication;
using Openlysis.MultiAnalyzer.Infrastructure.Orchestrators;
using Openlysis.MultiAnalyzer.Infrastructure.TimeoutRequests;

namespace Openlysis.MultiAnalyzer.Infrastructure;

/// <summary>
/// Provides extension methods for registering multi-analyzer worker infrastructure dependencies.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers the worker infrastructure services and configuration options
    /// required for multi-analysis orchestration.
    /// </summary>
    /// <param name="services">The service collection to add dependencies to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    internal static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var coordinatorOptionsSection = configuration
            .GetRequiredSection(OrchestrationOptions.SectionName);

        services
            .AddOptions<OrchestrationOptions>()
            .Bind(coordinatorOptionsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        AddUrlTimeoutRequestFactory(services, configuration);
        AddFileTimeoutRequestFactory(services, configuration);
    }

    /// <summary>
    /// Registers services and timeout request factories specific to URL analysis.
    /// </summary>
    /// <param name="services">The service collection to add dependencies to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    private static void AddUrlTimeoutRequestFactory(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<
            IUpdateMessageSender<UpdateMultiAnalysis<UrlAnalysis>>,
            UpdateMultiAnalysisMessageSender>();

        services.AddTransient<
            MultiAnalysisOrchestrator<UrlAnalysis, AnalyzeUrl>,
            UrlMultiAnalysisOrchestrator>();

        services.AddTimeoutRequestFactory<
            AnalyzeUrl,
            TimeoutAnalysisRequestCoreFactory<UrlAnalysis, AnalyzeUrl>>(
            configuration);
    }

    /// <summary>
    /// Registers services and timeout request factories specific to file analysis.
    /// </summary>
    /// <param name="services">The service collection to add dependencies to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    private static void AddFileTimeoutRequestFactory(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<
            IUpdateMessageSender<UpdateMultiAnalysis<FileAnalysis>>,
            UpdateMultiAnalysisMessageSender>();

        services.AddTransient<
            MultiAnalysisOrchestrator<FileAnalysis, AnalyzeFile>,
            FileMultiAnalysisOrchestrator>();

        services.AddTimeoutRequestFactory<
            AnalyzeFile,
            TimeoutAnalysisRequestCoreFactory<FileAnalysis, AnalyzeFile>>(
            configuration);
    }
}