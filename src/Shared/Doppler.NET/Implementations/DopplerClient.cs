using System.Text.Json;

using Doppler.NET.Abstractions;
using Doppler.NET.Constants;
using Doppler.NET.Models;

namespace Doppler.NET.Implementations;

/// <summary>
/// Provides an implementation of <see cref="IDopplerClient"/> for interacting with Doppler secrets API.
/// </summary>
internal sealed class DopplerClient : IDopplerClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="DopplerClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    /// <param name="serializerOptions">Options for JSON serialization.</param>
    internal DopplerClient(
        HttpClient httpClient,
        JsonSerializerOptions serializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = serializerOptions;
    }

    /// <inheritdoc/>
    public async Task<DopplerSecret?> GetSecretAsync(string project, string config, string secret)
    {
        var formattedUri = string.Format(
            DopplerApi.V3.Secrets.RetrievePath,
            project,
            config,
            secret);
        HttpResponseMessage response = await _httpClient.GetAsync(formattedUri).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string bodyString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<DopplerSecret>(bodyString, _jsonSerializerOptions);
    }
}