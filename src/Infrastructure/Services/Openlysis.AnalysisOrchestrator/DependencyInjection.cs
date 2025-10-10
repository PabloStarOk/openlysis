using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Openlysis.AnalysisOrchestrator.Infrastructure;

namespace Openlysis.AnalysisOrchestrator;

/// <summary>
/// Provides extension methods for registering worker-related services and analysis services
/// in the <see cref="IServiceCollection"/> for the MultiAnalyzer.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers all worker-related services.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="environment">The host environment instance.</param>
    internal static void AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddInfrastructure(configuration, environment);
        services.AddHostedService<AnalysisWorker>();
    }
}