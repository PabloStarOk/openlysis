using System.Text;

using ErrorOr;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Objects;
using Openlysis.Analyzers.Filescan.Core.Models.Requests;
using Openlysis.Analyzers.Filescan.Core.Models.Responses;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Analyzers.Filescan.Infrastructure.Analysis;

/// <summary>
/// Client to scan files.
/// </summary>
public sealed class FilescanAnalyzer : IFilescanAnalyzer
{
    private readonly IServiceLogger<FilescanAnalyzer> _serviceLogger;
    private readonly IServiceDeserializer _serviceDeserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilescanAnalyzer"/> class.
    /// </summary>
    /// <param name="serviceLogger">The analyzer logger instance for custom logging.</param>
    /// <param name="serviceDeserializer">The analyzer deserializer instance for custom deserialization.</param>
    public FilescanAnalyzer(
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceLogger<FilescanAnalyzer> serviceLogger,
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceDeserializer serviceDeserializer)
    {
        _serviceLogger = serviceLogger;
        _serviceDeserializer = serviceDeserializer;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ScanResponse>> AnalyzeAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default)
    {
        using FilescanAnalysisRequest request = requestFactory.Create();
        using HttpResponseMessage response = await httpClient.PostAsync(
            request.EndpointAddress,
            request.HttpContent,
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await _serviceLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        return await _serviceDeserializer
            .DeserializeAsync<ScanResponse>(response, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<GetAnalysisResponse>> GetAnalysisAsync(
        HttpClient httpClient,
        GetScanRequest getScanRequest,
        CancellationToken cancellationToken = default)
    {
        var uriBuilder = new UriBuilder(Addresses.BaseAddress)
        {
            Path = string.Format(Addresses.GetScan, getScanRequest.FlowId),
        };
        var paramsBuilder = new StringBuilder();

        if (getScanRequest.Filters?.Length > 0)
        {
            paramsBuilder.Append($"filter={string.Join(',', getScanRequest.Filters)}");
        }

        if (getScanRequest.Sorting?.Length > 0)
        {
            paramsBuilder.Append($"&sorting={string.Join(',', getScanRequest.Sorting)}");
        }

        if (getScanRequest.OtherQueryParams?.Length > 0)
        {
            paramsBuilder.Append($"&other={string.Join(',', getScanRequest.OtherQueryParams)}");
        }

        uriBuilder.Query = paramsBuilder.ToString();
        Uri requestUri = uriBuilder.Uri;

        using HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await _serviceLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        return await _serviceDeserializer
            .DeserializeAsync<GetAnalysisResponse>(response, cancellationToken);
    }
}