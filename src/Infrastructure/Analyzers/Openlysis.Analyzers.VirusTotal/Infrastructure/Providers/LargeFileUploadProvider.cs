using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Configuration;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Responses;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Analyzers.VirusTotal.Infrastructure.Providers;

/// <summary>
/// Provides functionality for obtaining URLs for uploading large files to the VirusTotal service.
/// This class facilitates the process of large file analysis by retrieving dedicated upload URLs.
/// </summary>
internal class LargeFileUploadProvider : ILargeFileUploadProvider
{
    private readonly IServiceLogger<LargeFileUploadProvider> _logger;
    private readonly IOptions<VirusTotalAnalyzerOptions> _analyzerOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="LargeFileUploadProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger for recording information and errors.</param>
    /// <param name="analyzerOptions">The configuration options for the VirusTotal analyzer.</param>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="deserializer">The service for deserializing HTTP responses.</param>
    public LargeFileUploadProvider(
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceLogger<LargeFileUploadProvider> logger,
        IOptions<VirusTotalAnalyzerOptions> analyzerOptions,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceDeserializer deserializer)
    {
        _logger = logger;
        _analyzerOptions = analyzerOptions;
        _httpClientFactory = httpClientFactory;
        _deserializer = deserializer;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<string>> GetFileUploadUrlAsync(
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = _httpClientFactory
            .CreateClient(_analyzerOptions.Value.ServiceName);

        using HttpResponseMessage response = await httpClient.GetAsync(
            Addresses.FileUploadUrlEndpoint,
            HttpCompletionOption.ResponseContentRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _logger.LogNonSuccessStatusCodeAsync(
                response,
                cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        ErrorOr<GetFileUploadUrl> deserializationResult =
            await _deserializer.DeserializeAsync<GetFileUploadUrl>(
            response,
            cancellationToken);

        if (deserializationResult.IsError)
        {
            return deserializationResult.Errors;
        }

        return deserializationResult.Value.Url;
    }
}