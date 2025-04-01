using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.Common.Constants;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.Contracts.Infrastructure.Logging.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Constants;
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;
using Openlysis.Analyzers.URLQuery.Core.Models.Requests;
using Openlysis.Analyzers.URLQuery.Core.Models.Responses;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Analyzers.URLQuery.Services;

/// <summary>
/// Analyzer of URLs using the 'urlquery.net' service.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    private readonly IOptionsMonitor<UrlQueryAnalyzerOptions> _urlQueryOptions;
    private readonly IVerdictCalculator _verdictCalculator;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter<Access>(JsonNamingPolicy.CamelCase),
            new JsonStringEnumConverter<Status>(JsonNamingPolicy.CamelCase),
            new JsonStringEnumConverter<Severity>(JsonNamingPolicy.CamelCase),
        },
    };

    private readonly IAnalyzerLogger _analyzerLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="UrlQueryAnalyzerOptions"/>.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="logger">The logger for logging information.</param>
    /// <param name="verdictCalculator">The calculator for determining the verdict of the URL analysis.</param>
    /// <param name="analyzerLogger">The logger for logging analyzer-specific information.</param>
    public UrlAnalyzer(
        IOptionsMonitor<UrlQueryAnalyzerOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<UrlAnalyzer> logger,
        IVerdictCalculator verdictCalculator,
        IAnalyzerLogger analyzerLogger)
        : base(options, httpClientFactory, logger)
    {
        _urlQueryOptions = options;
        _verdictCalculator = verdictCalculator;
        _analyzerLogger = analyzerLogger;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        string userAgent = Maps.UserAgentsMap[_urlQueryOptions.CurrentValue.DefaultUserAgent];
        var submitUrlRequest = new SubmitUrlRequest(
            request.Url.AbsoluteUri,
            userAgent,
            request.IsPrivate ? Access.Private : Access.Public);

        // Send request
        using var jsonContent = JsonContent.Create(
            submitUrlRequest,
            MediaTypeHeaderValue.Parse("application/json"),
            _jsonSerializerOptions);

        // Get response
        using HttpResponseMessage response = await httpClient.PostAsync(
            Addresses.SubmitUrlEndpoint,
            jsonContent,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        ErrorOr<SubmitUrlResponse> result;
        await using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            result = DeserializeResponse<SubmitUrlResponse>(jsonDocument.RootElement);
        }

        if (result.IsError)
        {
            return result.Errors;
        }

        // Return analysis
        SubmitUrlResponse submitUrlResponse = result.Value;
        AnalysisStatus status = Maps.AnalysisStatusMap[submitUrlResponse.Status];
        return UrlServiceAnalysis.Create(
            submitUrlResponse.ReportId,
            ServiceName,
            status,
            Verdict.Unknown,
            submitUrlResponse.QueueId);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id.Job);

        string formattedUrl = string.Format(Addresses.SubmitStatusEndpoint, id.Job);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

        ErrorOr<SubmitUrlResponse> result;
        await using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            result = DeserializeResponse<SubmitUrlResponse>(jsonDocument.RootElement);
        }

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value.Status];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        string formattedUrl = string.Format(Addresses.ReportEndpoint, id.Primary.Value);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return AnalyzerErrors.NonSuccessStatusCode;
        }

#if DEBUG
        _logger.LogDebug(
            "UrlQuery Analysis Report: {Body}",
            await response.Content.ReadAsStringAsync(cancellationToken));
#endif

        ErrorOr<GetReportResponse> reportResult;
        await using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            reportResult = DeserializeResponse<GetReportResponse>(jsonDocument.RootElement);
        }

        if (reportResult.IsError)
        {
            return reportResult.Errors;
        }

        GetReportResponse report = reportResult.Value;

#if DEBUG
        _logger.LogDebug(
            "UrlQuery Report Sensors:\n{Body}",
            report.Sensors);
#endif

        AnalysisStatus status = Maps.AnalysisStatusMap[report.Status];
        Verdict verdict = _verdictCalculator.Calculate(report.Sensors);
        return UrlServiceAnalysis.Create(report.ReportId, ServiceName, status, verdict, id.Job);
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