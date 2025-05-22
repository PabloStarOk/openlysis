using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.EmailAddresses;
using Openlysis.TestTools.ServicesSimulation.Files;
using Openlysis.TestTools.ServicesSimulation.PhonesNumbers;
using Openlysis.TestTools.ServicesSimulation.URLs;

namespace Openlysis.TestTools.ServicesSimulation;

/// <summary>
/// Provides extension methods for configuring simulated analysis and reputation services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers simulated analysis services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the simulated services to.</param>
    /// <param name="configuration">The configuration that contains settings for the simulated services.</param>
    public static void AddSimulatedAnalysisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<AnalysisStubFactoryOptions>,
            AnalysisStubFactoryOptionsValidator>();

        using (var sp = services.BuildServiceProvider())
        {
            if (sp.GetService<TimeProvider>() is not null)
            {
                return;
            }

            services.AddSingleton(TimeProvider.System);
        }

        ILogger logger = CreateLogger(services);
        services.AddSimulatedFileAnalyzers(configuration, logger);
        services.AddSimulatedUrlAnalyzers(configuration, logger);
    }

    /// <summary>
    /// Registers simulated reputation services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the simulated reputation services to.</param>
    /// <param name="configuration">The configuration that contains settings for the simulated reputation services.</param>
    public static void AddSimulatedReputationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ILogger logger = CreateLogger(services);
        services.AddSimulatedEmailReputationServices(configuration, logger);
        services.AddSimulatedPhoneReputationServices(configuration, logger);
    }

    /// <summary>
    /// Creates a logger for the simulated services.
    /// </summary>
    /// <param name="services">The service collection to create the logger from.</param>
    /// <returns>An <see cref="ILogger"/> instance configured for the simulated services.</returns>
    private static ILogger CreateLogger(IServiceCollection services)
    {
        using ServiceProvider sp = services.BuildServiceProvider();
        return sp.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DependencyInjection).Assembly.GetName().Name
                ?? nameof(ServicesSimulation));
    }
}