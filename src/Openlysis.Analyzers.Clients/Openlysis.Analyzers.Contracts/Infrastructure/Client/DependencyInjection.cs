using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Contracts.Core.Configuration;

namespace Openlysis.Analyzers.Contracts.Infrastructure.Client;

/// <summary>
/// Provides extension methods for adding HTTP clients to the service collection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds an <see cref="HttpClient"/> singleton instance to the <see cref="IServiceCollection"/> with the specified options.
    /// </summary>
    /// <param name="services">The service collection to add the <see cref="HttpClient"/> to.</param>
    /// <param name="secretOptions">The secret options containing the API key.</param>
    /// <param name="analyzerOptions">The analyzer options containing the base address and timeout settings.</param>
    public static void ConfigureHttpClient(
        this IServiceCollection services,
        SecretOptions secretOptions,
        AnalyzerOptions analyzerOptions)
    {
        services.AddHttpClient(analyzerOptions.ServiceName, httpClient =>
        {
            httpClient.BaseAddress = analyzerOptions.BaseAddress;
            httpClient.Timeout = TimeSpan.FromMilliseconds(analyzerOptions.RequestsTimeoutMs);
            httpClient.DefaultRequestHeaders.Add(analyzerOptions.ApiKeyHeaderName, secretOptions.ApiKey);
            httpClient.DefaultRequestVersion = HttpVersion.Version30;
        });
    }

    /// <summary>
    /// Adds an <see cref="HttpClient"/> singleton instance to the <see cref="IServiceCollection"/> with the specified options and configuration action.
    /// </summary>
    /// <param name="services">The service collection to add the <see cref="HttpClient"/> to.</param>
    /// <param name="secretOptions">The secret options containing the API key.</param>
    /// <param name="analyzerOptions">The analyzer options containing the base address and timeout settings.</param>
    /// <param name="configureClient">An action to configure the <see cref="HttpClient"/>.</param>
    public static void ConfigureHttpClient(
        this IServiceCollection services,
        SecretOptions secretOptions,
        AnalyzerOptions analyzerOptions,
        Action<HttpClient> configureClient)
    {
        services.AddHttpClient(analyzerOptions.ServiceName, httpClient =>
        {
            httpClient.BaseAddress = analyzerOptions.BaseAddress;
            httpClient.Timeout = TimeSpan.FromMilliseconds(analyzerOptions.RequestsTimeoutMs);
            httpClient.DefaultRequestHeaders.Add(analyzerOptions.ApiKeyHeaderName, secretOptions.ApiKey);
            httpClient.DefaultRequestVersion = HttpVersion.Version30;
            configureClient(httpClient);
        });
    }
}