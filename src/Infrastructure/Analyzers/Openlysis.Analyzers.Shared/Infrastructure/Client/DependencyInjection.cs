using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

namespace Openlysis.Analyzers.Shared.Infrastructure.Client;

/// <summary>
/// Provides extension methods for adding HTTP clients to the service collection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configures and registers an <see cref="HttpClient"/> for the specified analyzer service.
    /// </summary>
    /// <param name="services">The service collection to add the HTTP client to.</param>
    /// <param name="apiKeySecretName">The name of the secret containing the API key.</param>
    /// <param name="analyzerOptions">Options for configuring the analyzer client.</param>
    /// <param name="configure">Optional action to further configure the <see cref="HttpClient"/>.</param>
    public static void ConfigureHttpClient(
        this IServiceCollection services,
        string apiKeySecretName,
        AnalyzerOptions analyzerOptions,
        Action<HttpClient>? configure = null)
    {
        services.AddHttpClient(analyzerOptions.ServiceName, (sp, httpClient) =>
        {
            var apiKeyProvider = sp.GetRequiredService<IApiKeyProvider>();
            var apiKey = apiKeyProvider.GetApiKey(apiKeySecretName);

            httpClient.BaseAddress = analyzerOptions.BaseAddress;
            httpClient.Timeout = TimeSpan.FromMilliseconds(analyzerOptions.RequestsTimeoutMs);
            httpClient.DefaultRequestHeaders.Add(analyzerOptions.ApiKeyHeaderName, apiKey);
            httpClient.DefaultRequestVersion = HttpVersion.Version30;
            configure?.Invoke(httpClient);
        });
    }
}