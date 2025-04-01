using System.Text.Json;
using System.Text.Json.Serialization;

using ErrorOr;

using Openlysis.Analyzers.Contracts.Core.Common.Constants;
using Openlysis.Analyzers.Contracts.Infrastructure.Logging.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Enums;
using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;
using Openlysis.Analyzers.VirusTotal.Core.Models.Responses;

namespace Openlysis.Analyzers.VirusTotal.Infrastructure.Services;

/// <summary>
/// Analyzes URLs and retrieves analysis results from VirusTotal.
/// </summary>
public class VirusTotalAnalyzer : IVirusTotalAnalyzer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter<Status>(JsonNamingPolicy.KebabCaseLower),
        },
    };

    private readonly IAnalyzerLogger _analyzerLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirusTotalAnalyzer"/> class.
    /// </summary>
    /// <param name="analyzerLogger">The analyzer logger instance to use for logging analysis-specific information.</param>
    public VirusTotalAnalyzer(IAnalyzerLogger analyzerLogger)
    {
        _analyzerLogger = analyzerLogger;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<AnalyzeUrlResponse>> AnalyzeAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestFactory);
        using VirusTotalAnalysisRequest request = requestFactory.Create();

        using HttpResponseMessage response = await httpClient.PostAsync(
            request.Address,
            request.Content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        JsonElement dataElement;
        await using (Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            dataElement = jsonDocument.RootElement.GetProperty("data").Clone();
        }

        return DeserializeResponse<AnalyzeUrlResponse>(dataElement);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<GetAnalysisResponse>> GetAnalysisAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default)
    {
        string formattedUrl = string.Format(Addresses.AnalysesEndpoint, id);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        JsonElement dataElement;
        await using (Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            dataElement = jsonDocument.RootElement.GetProperty("data").Clone();
        }

        return DeserializeResponse<GetAnalysisResponse>(dataElement);
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