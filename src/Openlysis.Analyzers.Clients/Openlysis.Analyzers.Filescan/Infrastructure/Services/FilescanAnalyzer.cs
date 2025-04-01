using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using ErrorOr;

using Openlysis.Analyzers.Contracts.Core.Common.Constants;
using Openlysis.Analyzers.Contracts.Infrastructure.Logging.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Analyzers.Filescan.Core.Models.Objects;
using Openlysis.Analyzers.Filescan.Core.Models.Requests;
using Openlysis.Analyzers.Filescan.Core.Models.Responses;

namespace Openlysis.Analyzers.Filescan.Infrastructure.Services;

/// <summary>
/// Client to scan files.
/// </summary>
public sealed class FilescanAnalyzer : IFilescanAnalyzer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter<Status>(JsonNamingPolicy.CamelCase),
            new JsonStringEnumConverter<FilescanVerdict>(JsonNamingPolicy.SnakeCaseUpper),
        },
    };

    private readonly IAnalyzerLogger _analyzerLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilescanAnalyzer"/> class.
    /// </summary>
    /// <param name="analyzerLogger">The analyzer logger instance for custom logging.</param>
    public FilescanAnalyzer(IAnalyzerLogger analyzerLogger)
    {
        _analyzerLogger = analyzerLogger;
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
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return DeserializeResponse<ScanResponse>(jsonDocument.RootElement);
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
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return DeserializeResponse<GetAnalysisResponse>(jsonDocument.RootElement);
    }

    /// <summary>
    /// Deserializes the HTTP response content to a specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to deserialize to.</typeparam>
    /// <param name="jsonElement">The JSON element containing the response data.</param>
    /// <returns>An <see cref="ErrorOr{TModel}"/> containing the deserialized model or an error.</returns>
    private ErrorOr<TModel> DeserializeResponse<TModel>(JsonElement jsonElement)
        where TModel : notnull
    {
        TModel? model;
        try
        {
            model = jsonElement.Deserialize<TModel>(_jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            _analyzerLogger.LogDeserializationFailure(typeof(TModel), ex, jsonElement);
            return AnalyzerErrors.DeserializationFailure;
        }

        if (model is not null)
        {
            return model;
        }

        _analyzerLogger.LogUnexpectedNullResult(typeof(TModel));
        return AnalyzerErrors.DeserializationNull;
    }
}