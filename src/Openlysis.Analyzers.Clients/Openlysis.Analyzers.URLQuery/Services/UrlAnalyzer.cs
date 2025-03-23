using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Constants;
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;
using Openlysis.Analyzers.URLQuery.Core.Models.Objects;
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
        },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="UrlQueryAnalyzerOptions"/>.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="logger">The logger for logging information.</param>
    /// <param name="verdictCalculator">The calculator for determining the verdict of the URL analysis.</param>
    public UrlAnalyzer(
        IOptionsMonitor<UrlQueryAnalyzerOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<UrlAnalyzer> logger,
        IVerdictCalculator verdictCalculator)
        : base(options, httpClientFactory, logger)
    {
        _urlQueryOptions = options;
        _verdictCalculator = verdictCalculator;
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
            return await LogAndReturnStatusCodeErrorAsync(response, cancellationToken);
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
            return await LogAndReturnStatusCodeErrorAsync(response, cancellationToken);
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
        string formattedUrl = string.Format(Addresses.ReportOverviewEndpoint, id.Primary.Value);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return await LogAndReturnStatusCodeErrorAsync(response, cancellationToken);
        }

        ErrorOr<GetReportOverviewResponse> reportResult;
        ErrorOr<Stats> statsResult;
        await using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken))
        {
            reportResult = DeserializeResponse<GetReportOverviewResponse>(jsonDocument.RootElement);
            statsResult = ParseStats(jsonDocument.RootElement);
        }

        if (reportResult.IsError)
        {
            return reportResult.Errors;
        }

        if (statsResult.IsError)
        {
            return statsResult.Errors;
        }

        GetReportOverviewResponse overviewResponse = reportResult.Value;
        Stats stats = statsResult.Value;

#if DEBUG
        _logger.LogDebug(
            "UrlQuery Results:"
            + "\n\tUrlQuery Alerts: {UrlQuery}"
            + "\n\tNetwork Intrusion Detection Alerts: {Ids}"
            + "\n\tThreat Detection System Alerts: {Tds}",
            stats.UrlQueryAlerts,
            stats.IdsAlerts,
            stats.ThreatDetectionSystemsAlerts);
#endif

        AnalysisStatus status = Maps.AnalysisStatusMap[overviewResponse.Status];
        Verdict verdict = _verdictCalculator.Calculate(stats);
        return UrlServiceAnalysis.Create(overviewResponse.ReportId, ServiceName, status, verdict, id.Job);
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
            return LogAndReturnDeserializationException(ex, typeof(TModel));
        }

        return model is null ? LogAndReturnNullError(typeof(TModel)) : model;
    }

    /// <summary>
    /// Parses the statistics from the HTTP response.
    /// </summary>
    /// <param name="rootElement">The root JSON element containing the response data.</param>
    /// <returns>An <see cref="ErrorOr{Stats}"/> containing the parsed statistics or an error.</returns>
    private ErrorOr<Stats> ParseStats(JsonElement rootElement)
    {
        Stats? stats;
        try
        {
            stats = rootElement
                .GetProperty("stats")
                .GetProperty("alert_count")
                .Deserialize<Stats>(_jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return LogAndReturnDeserializationException(ex, typeof(Stats));
        }

        if (stats is not null)
        {
            return stats;
        }

        return stats is null ? LogAndReturnNullError(typeof(Stats)) : stats;
    }

    /// <summary>
    /// Logs an error for an unsuccessful HTTP response status code and returns an error.
    /// </summary>
    /// <param name="response">The HTTP response message containing the error status code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="Error"/> indicating the unsuccessful status code.</returns>
    private async Task<Error> LogAndReturnStatusCodeErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var stringBuilder = new StringBuilder();

        if (response.RequestMessage?.Method is not null)
        {
            stringBuilder.AppendLine($"Method: {response.RequestMessage.Method}");
        }

        if (response.RequestMessage?.RequestUri is not null)
        {
            stringBuilder.AppendLine($"Request URI: {response.RequestMessage.RequestUri.AbsoluteUri}");
        }

        if (response.RequestMessage is not null)
        {
            bool containsApiKeyHeader = response.RequestMessage.Headers
                .Any(h => h.Key == _urlQueryOptions.CurrentValue.ApiKeyHeaderName
                    && !string.IsNullOrWhiteSpace(h.Value.ToString()));
            stringBuilder.AppendLine($"Contains API Key: {containsApiKeyHeader.ToString()}");
        }

        if (response.RequestMessage?.Content is not null)
        {
            string requestString = await response.RequestMessage.Content.ReadAsStringAsync(cancellationToken);
            stringBuilder.AppendLine($"Request Body: {requestString}");
        }

        _logger.LogError(
            "Response status code was not successful: \nStatusCode: {StatusCode}\nResponse: {Response}\n{Request}",
            response.StatusCode,
            responseString,
            stringBuilder.ToString());
        return Error.Unexpected("Response.NotSuccessful", "Response status code was not successful.");
    }

    /// <summary>
    /// Logs an exception and returns an error indicating a deserialization exception.
    /// </summary>
    /// <param name="exception">The exception that was caught during deserialization.</param>
    /// <param name="type">The type of the response that was being deserialized.</param>
    /// <returns>An <see cref="Error"/> indicating a deserialization exception.</returns>
    private Error LogAndReturnDeserializationException(Exception exception, Type type)
    {
        _logger.LogError(
            exception,
            "Exception caught while trying to deserialize a response of type {ResponseType} at {ServiceName}",
            type.Name,
            ServiceName);
        return Error.Unexpected("Response.DeserializationError", "Exception caught while trying to deserialize a response.");
    }

    /// <summary>
    /// Logs an error indicating that the deserialized object was null and returns an error.
    /// </summary>
    /// <param name="type">The type of the response that was being deserialized.</param>
    /// <returns>An <see cref="Error"/> indicating a null object after deserialization.</returns>
    private Error LogAndReturnNullError(Type type)
    {
        _logger.LogError(
            "{ResponseType} was null after deserialization at {ServiceName} analyzer service.",
            type.Name,
            ServiceName);
        return Error.Unexpected("Response.NullDeserialization", "An object was null after deserialization.");
    }
}