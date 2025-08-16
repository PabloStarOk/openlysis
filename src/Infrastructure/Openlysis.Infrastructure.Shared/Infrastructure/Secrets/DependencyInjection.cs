using Doppler.NET;
using Doppler.NET.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

/// <summary>
/// Provides extension methods for registering Doppler API key provider dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the DopplerApiKeyProvider and related services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the provider to.</param>
    /// <param name="dopplerOptions">Options for configuring the Doppler client.</param>
    /// <param name="configure">Delegate to configure DopplerApiKeyProviderOptions.</param>
    public static void AddDopplerApiKeyProvider(
        this IServiceCollection services,
        DopplerClientOptions dopplerOptions,
        Action<DopplerApiKeyProviderOptions> configure)
    {
        services.AddSingleton<
            IValidateOptions<DopplerApiKeyProviderOptions>,
            ApiKeyProviderOptionsValidator>();

        services
            .AddOptions<DopplerApiKeyProviderOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.AddDopplerClient(options =>
        {
            options.ServiceTokenEnvVariable = dopplerOptions.ServiceTokenEnvVariable;
            options.ProjectName = dopplerOptions.ProjectName;
            options.ConfigName = dopplerOptions.ConfigName;
        });
        services.AddSingleton<DopplerApiKeyProvider>();
        services.AddHostedService(sp => sp.GetRequiredService<DopplerApiKeyProvider>());
        services.AddSingleton<IApiKeyProvider>(sp => sp.GetRequiredService<DopplerApiKeyProvider>());
    }
}