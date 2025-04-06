using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.Shared.Core.Common.Constants;
using Openlysis.Analyzers.Shared.Infrastructure.Deserialization.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;
using Openlysis.Analyzers.VirusTotal.Core.Models.Responses;
using Openlysis.Infrastructure.Shared.Logging.Abstractions;

namespace Openlysis.Analyzers.VirusTotal.Infrastructure.Services;

/// <summary>
/// Analyzes URLs and retrieves analysis results from VirusTotal.
/// </summary>
public class VirusTotalAnalyzer : IVirusTotalAnalyzer
{
    /// <summary>
    /// Key used to identify VirusTotal services.
    /// </summary>
    public const string KeyedServicesKey = "VirusTotalServices";

    private readonly ServiceLogger _serviceLogger;
    private readonly IAnalyzerDeserializer _analyzerDeserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirusTotalAnalyzer"/> class.
    /// </summary>
    /// <param name="serviceLogger">The analyzer logger instance to use for logging analysis-specific information.</param>
    /// <param name="analyzerDeserializer">The analyzer deserializer instance to use for deserializing analysis responses.</param>
    public VirusTotalAnalyzer(
        [FromKeyedServices(KeyedServicesKey)] ServiceLogger serviceLogger,
        [FromKeyedServices(KeyedServicesKey)] IAnalyzerDeserializer analyzerDeserializer)
    {
        _serviceLogger = serviceLogger;
        _analyzerDeserializer = analyzerDeserializer;
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
            await _serviceLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        JsonElement dataElement;
        await using (Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            dataElement = jsonDocument.RootElement.GetProperty("data").Clone();
        }

        return _analyzerDeserializer.Deserialize<AnalyzeUrlResponse>(dataElement);
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
            await _serviceLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        JsonElement dataElement;
        await using (Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (JsonDocument jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            dataElement = jsonDocument.RootElement.GetProperty("data").Clone();
        }

        return _analyzerDeserializer.Deserialize<GetAnalysisResponse>(dataElement);
    }
}