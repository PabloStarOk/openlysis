using System.Net.Http.Headers;

using Doppler.NET.Abstractions;
using Doppler.NET.Configuration;
using Doppler.NET.Constants;
using Doppler.NET.Implementations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Doppler.NET;

/// <summary>
/// Provides extension methods for registering Doppler client dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Doppler client and its configuration in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the Doppler client to.</param>
    /// <param name="configure">An action to configure DopplerClientOptions.</param>
    public static void AddDopplerClient(
        this IServiceCollection services,
        Action<DopplerClientOptions> configure)
    {
        services.AddSingleton<IValidateOptions<DopplerClientOptions>, DopplerClientOptionsValidator>();
        services.AddOptions<DopplerClientOptions>().Configure(configure).ValidateOnStart();
        services.AddHttpClient<IDopplerClient, DopplerClient>((client, sp) =>
        {
            var options = sp.GetRequiredService<IOptions<DopplerClientOptions>>();
            string? serviceToken = Environment.GetEnvironmentVariable(options.Value.ServiceTokenEnvVariable);
            if (string.IsNullOrWhiteSpace(serviceToken))
            {
                throw new InvalidOperationException($"Service token is missing. Ensure the environment variable '{options.Value.ServiceTokenEnvVariable}' is set.");
            }

            var authorizationHeaderValue = new AuthenticationHeaderValue(
                DopplerApi.AuthScheme,
                serviceToken);

            client.BaseAddress = new Uri(DopplerApi.V3.BaseUrl);
            client.DefaultRequestHeaders.Authorization = authorizationHeaderValue;

            return new DopplerClient(options, client);
        });
    }
}