using Doppler.NET.Abstractions;
using Doppler.NET.Models;

using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Communication.Services.Files;

namespace Openlysis.Infrastructure.Shared.Infrastructure.Secrets;

/// <summary>
/// Provides secrets from Doppler, including API keys and Google Cloud credentials.
/// Implements <see cref="IApiKeyProvider"/>, <see cref="IGoogleCloudCredentialProvider"/>, and <see cref="IHostedService"/>.
/// </summary>
internal sealed class DopplerSecretsProvider
    : IApiKeyProvider, IGoogleCloudCredentialProvider, IHostedService
{
    /// <inheritdoc/>
    public ICredential Credential
    {
        get
        {
            if (_credential is null)
            {
                throw new InvalidOperationException("Credential has not been initialized.");
            }

            return _credential;
        }
    }

    private readonly IOptions<DopplerSecretsProviderOptions> _options;
    private readonly IDopplerClient _dopplerClient;
    private readonly Dictionary<string, string> _apiKeys = [];
    private ICredential? _credential;

    /// <summary>
    /// Initializes a new instance of the <see cref="DopplerSecretsProvider"/> class.
    /// </summary>
    /// <param name="options">The options containing Doppler secrets provider configuration.</param>
    /// <param name="dopplerClient">The Doppler client used to fetch secrets.</param>
    public DopplerSecretsProvider(
        IOptions<DopplerSecretsProviderOptions> options,
        IDopplerClient dopplerClient)
    {
        _options = options;
        _dopplerClient = dopplerClient;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Task gcsCredentialTask = FetchGcsCredentialAsync(cancellationToken);
        Task apiKeysTask = Parallel.ForEachAsync(
            _options.Value.ApiKeySecretNames,
            cancellationToken,
            FetchApiKeyAsync);

        await Task.WhenAll(gcsCredentialTask, apiKeysTask);
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

    private async Task FetchGcsCredentialAsync(CancellationToken cancellationToken)
    {
        DopplerSecret? secret = await _dopplerClient.GetSecretAsync(
            _options.Value.GcsCredentialSecretName,
            cancellationToken);
        if (secret is null || string.IsNullOrWhiteSpace(secret.Value.Raw))
        {
            throw new InvalidOperationException("Google Cloud Storage credential JSON is missing or empty.");
        }

        _credential = GoogleCredential.FromJson(secret.Value.Raw);
    }
}