using System.Text.Json;

using Doppler.NET.Abstractions;
using Doppler.NET.Configuration;
using Doppler.NET.Constants;
using Doppler.NET.Models;

using Microsoft.Extensions.Options;

namespace Doppler.NET.Implementations;

/// <summary>
/// Provides an implementation of <see cref="IDopplerClient"/> for interacting with Doppler secrets API.
/// </summary>
internal sealed class DopplerClient : IDopplerClient
{
    private readonly IOptions<DopplerClientOptions> _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DopplerClient"/> class.
    /// </summary>
    /// <param name="options">The Doppler client options.</param>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    internal DopplerClient(
        IOptions<DopplerClientOptions> options,
        HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<DopplerSecret?> GetSecretAsync(
        string secret,
        CancellationToken cancellationToken = default)
    {
        var formattedUri = string.Format(
            DopplerApi.V3.Secrets.RetrievePath,
            _options.Value.ProjectName,
            _options.Value.ConfigName,
            secret);
        HttpResponseMessage response = await _httpClient
            .GetAsync(formattedUri, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string bodyString = await response.Content
            .ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<DopplerSecret>(bodyString, _options.Value.SerializerOptions);
    }
}