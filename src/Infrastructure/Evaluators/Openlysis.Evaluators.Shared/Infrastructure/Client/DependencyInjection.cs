using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Evaluators.Shared.Infrastructure.Client;

/// <summary>
/// Provides extension methods for adding HTTP clients to the service collection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds an <see cref="HttpClient"/> singleton instance to the <see cref="IServiceCollection"/> with the specified options.
    /// </summary>
    /// <param name="services">The service collection to add the <see cref="HttpClient"/> to.</param>
    /// <param name="evaluatorOptions">The options for configuring the evaluator.</param>
    public static void ConfigureHttpClient(
        this IServiceCollection services,
        ServiceOptions evaluatorOptions)
    {
        services.AddHttpClient(evaluatorOptions.ServiceName, httpClient =>
        {
            httpClient.BaseAddress = evaluatorOptions.BaseAddress;
            httpClient.Timeout = TimeSpan.FromMilliseconds(evaluatorOptions.RequestsTimeoutMs);
            httpClient.DefaultRequestVersion = HttpVersion.Version30;
        });
    }
}