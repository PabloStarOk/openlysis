using Doppler.NET;
using Doppler.NET.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Communication.Services.Files;

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
    public static void AddDopplerSecretsProvider(
        this IServiceCollection services,
        DopplerClientOptions dopplerOptions,
        Action<DopplerSecretsProviderOptions> configure)
    {
        services.AddSingleton<
            IValidateOptions<DopplerSecretsProviderOptions>,
            ApiKeyProviderOptionsValidator>();

        services
            .AddOptions<DopplerSecretsProviderOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.AddDopplerClient(options =>
        {
            options.ServiceTokenEnvVariable = dopplerOptions.ServiceTokenEnvVariable;
            options.ProjectName = dopplerOptions.ProjectName;
            options.ConfigName = dopplerOptions.ConfigName;
        });
        services.AddSingleton<DopplerSecretsProvider>();
        services.AddHostedService(sp => sp.GetRequiredService<DopplerSecretsProvider>());
        services.AddSingleton<IApiKeyProvider>(sp => sp.GetRequiredService<DopplerSecretsProvider>());
        services.AddSingleton<IGoogleCloudCredentialProvider>(sp => sp.GetRequiredService<DopplerSecretsProvider>());
    }
}