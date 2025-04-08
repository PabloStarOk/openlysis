using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Assessors.Shared.Configuration;
using Openlysis.Infrastructure.Shared.Configuration;

namespace Openlysis.Assessors.Shared.Infrastructure.Client;

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
    /// <param name="assessorOptions">The options for configuring the data assessor.</param>
    public static void ConfigureHttpClient(
        this IServiceCollection services,
        SecretOptions secretOptions,
        DataAssessorOptions assessorOptions)
    {
        services.AddHttpClient(assessorOptions.ServiceName, httpClient =>
        {
            httpClient.BaseAddress = assessorOptions.BaseAddress;
            httpClient.Timeout = TimeSpan.FromMilliseconds(assessorOptions.RequestsTimeoutMs);
            httpClient.DefaultRequestVersion = HttpVersion.Version30;
        });
    }
}