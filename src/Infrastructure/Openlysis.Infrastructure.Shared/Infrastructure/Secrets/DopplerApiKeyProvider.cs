using Doppler.NET.Abstractions;
using Doppler.NET.Models;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

/// <summary>
/// Provides API keys by fetching secrets from Doppler and implements hosted service lifecycle.
/// </summary>
internal sealed class DopplerApiKeyProvider : IApiKeyProvider, IHostedService
{
    private readonly IOptions<DopplerApiKeyProviderOptions> _options;
    private readonly IDopplerClient _dopplerClient;
    private readonly Dictionary<string, string> _apiKeys = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DopplerApiKeyProvider"/> class.
    /// </summary>
    /// <param name="options">The options containing Doppler API key provider configuration.</param>
    /// <param name="dopplerClient">The Doppler client used to fetch secrets.</param>
    public DopplerApiKeyProvider(
        IOptions<DopplerApiKeyProviderOptions> options,
        IDopplerClient dopplerClient)
    {
        _options = options;
        _dopplerClient = dopplerClient;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            _options.Value.ApiKeySecretNames,
            cancellationToken,
            FetchApiKeyAsync);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public string GetApiKey(string apiKeySecretName)
    {
        return _apiKeys[apiKeySecretName];
    }

    private async ValueTask FetchApiKeyAsync(
        string apiKeySecretName,
        CancellationToken cancellationToken)
    {
        DopplerSecret? apiKeySecret = await _dopplerClient
            .GetSecretAsync(apiKeySecretName, cancellationToken)
            .ConfigureAwait(false);

        if (apiKeySecret is null)
        {
            throw new InvalidOperationException("Failed to retrieve API key secret from Doppler.");
        }

        if (string.IsNullOrWhiteSpace(apiKeySecret.Value.Raw))
        {
            throw new InvalidOperationException("API key value from Doppler is empty or whitespace.");
        }

        _apiKeys.Add(apiKeySecretName, apiKeySecret.Value.Raw);
    }
}