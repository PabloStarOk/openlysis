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
    /// <param name="serviceToken">The service token used for authentication with Doppler project.</param>
    /// <param name="configure">An action to configure DopplerClientOptions.</param>
    public static void AddDopplerClient(
        this IServiceCollection services,
        string serviceToken,
        Action<DopplerClientOptions> configure)
    {
        services.AddOptions<DopplerClientOptions>().Configure(options =>
        {
            configure(options);
            options.ServiceToken = serviceToken;
        });

        services.AddHttpClient<IDopplerClient, DopplerClient>((client, sp) =>
        {
            var options = sp.GetRequiredService<IOptions<DopplerClientOptions>>();
            var authorizationHeaderValue = new AuthenticationHeaderValue(
                DopplerApi.AuthScheme,
                options.Value.ServiceToken);

            client.BaseAddress = new Uri(DopplerApi.V3.BaseUrl);
            client.DefaultRequestHeaders.Authorization = authorizationHeaderValue;

            return new DopplerClient(
                options,
                client);
        });
    }
}