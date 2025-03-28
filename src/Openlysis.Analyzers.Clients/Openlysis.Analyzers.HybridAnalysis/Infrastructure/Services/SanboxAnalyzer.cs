using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Services;

/// <summary>
/// Represents an analyzer which sends request to Hybrid Analysis to analyze Files or URLs.
/// </summary>
public class SandboxAnalyzer : ISandboxAnalyzer
{
    private readonly ILogger<SandboxAnalyzer> _logger;
    private readonly IOptionsMonitor<HybridAnalyzerOptions> _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter<Status>(JsonNamingPolicy.SnakeCaseUpper),
        },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log messages.</param>
    /// <param name="options">The options monitor instance to access configuration settings.</param>
    public SandboxAnalyzer(
        ILogger<SandboxAnalyzer> logger,
        IOptionsMonitor<HybridAnalyzerOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<SandboxSubmitResponse>> AnalyzeAsync<TContent>(
        HttpClient httpClient,
        SandboxSubmitRequest<TContent> request,
        CancellationToken cancellationToken = default)
        where TContent : notnull
    {
        using HttpContent httpContent = GetHttpContentRequest(request, out string endpointAddress);
        using HttpResponseMessage response = await httpClient.PostAsync(
            endpointAddress,
            httpContent,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await LogStatusCodeErrorAsync(response, cancellationToken);

            return response.StatusCode is HttpStatusCode.TooManyRequests
                ? Error.Failure(code: ErrorCodes.TooManyRequests)
                : GetUnexpectedStatusCodeError();
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return DeserializeResponse<SandboxSubmitResponse>(jsonDocument.RootElement);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<Status>> GetReportStatusAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default)
    {
        var formattedAddress = string.Format(Addresses.SandboxReportStateEndpoint, id);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedAddress, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await LogStatusCodeErrorAsync(response, cancellationToken);
            return GetUnexpectedStatusCodeError();
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        ErrorOr<ReportStateResponse> result = DeserializeResponse<ReportStateResponse>(jsonDocument.RootElement);
        if (result.IsError)
        {
            return result.Errors;
        }

        ReportStateResponse reportState = result.Value;
        if (reportState.Status is not Status.Error)
        {
            return reportState.Status;
        }

        string requestLog = await GetRequestLogAsync(response, cancellationToken);
        _logger.LogError(
            "Report status was Error at {ServiceName} sandbox service analyzer."
            + "\nRequest:"
            + "\t\n{Request}"
            + "\nHybrid Analysis Response:"
            + "\n\tError Type: {ErrorType}"
            + "\n\tError Origin: {ErrorOrigin}"
            + "\n\tError Description: {Error}",
            _options.CurrentValue.ServiceName,
            requestLog,
            reportState.ErrorType,
            reportState.ErrorOrigin,
            reportState.Error);

        return reportState.Status;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<SanboxReportSummary>> GetReportSummaryAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default)
    {
        var formattedAddress = string.Format(Addresses.SandboxReportSummary, id);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedAddress, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await LogStatusCodeErrorAsync(response, cancellationToken);
            return GetUnexpectedStatusCodeError();
        }

        // 4. Map report.
        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return DeserializeResponse<SanboxReportSummary>(jsonDocument.RootElement);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<string>> CheckUrlHashAsync(
        HttpClient httpClient,
        Uri url,
        CancellationToken cancellationToken = default)
    {
        var dictionary = new Dictionary<string, string>(1)
        {
            { "url", url.AbsoluteUri },
        };
        using var formUrlEncoded = new FormUrlEncodedContent(dictionary);
        using HttpResponseMessage response = await httpClient.PostAsync(Addresses.SubmitHashForUrl, formUrlEncoded, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return GetUnexpectedStatusCodeError();
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        string? hash = jsonDocument.RootElement.GetProperty("sha256").GetString();

        if (hash is null)
        {
            throw new InvalidOperationException("Hash was null when checking SHA-256 for a URL at Hybrid Analysis Sandbox service.");
        }

        return hash;
    }

    /// <summary>
    /// Returns an error indicating that the HTTP response status code was not successful.
    /// </summary>
    /// <returns>An <see cref="Error"/> indicating an unsuccessful response status code.</returns>
    private static Error GetUnexpectedStatusCodeError()
    {
        return Error.Unexpected("Response.NotSuccessful", "Response status code was not successful.");
    }

    /// <summary>
    /// Creates an HTTP content request for the sandbox analyzer.
    /// </summary>
    /// <typeparam name="TContent">The type of the content to be analyzed.</typeparam>
    /// <param name="request">The request containing the content to be analyzed.</param>
    /// <param name="endpointAddress">The endpoint address to which the request will be sent.</param>
    /// <returns>The HTTP content to be sent in the request.</returns>
    private HttpContent GetHttpContentRequest<TContent>(
        SandboxSubmitRequest<TContent> request,
        out string endpointAddress)
        where TContent : notnull
    {
        if (request is not SubmitUrlRequest urlRequest)
        {
            throw new NotImplementedException();
        }

        endpointAddress = Addresses.SandboxSubmitUrlEndpoint;
        int environmentId = (int)_options.CurrentValue.DefaultSandboxEnvironment;
        var dictionary = new Dictionary<string, string>
        {
            { "url", urlRequest.Url.AbsoluteUri },
            { "environment_id", environmentId.ToString() },
        };
        return new FormUrlEncodedContent(dictionary);
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
            return LogAndReturnDeserializationException(ex, typeof(TModel), jsonElement.GetRawText());
        }

        return model is null ? LogAndReturnNullError(typeof(TModel)) : model;
    }

    /// <summary>
    /// Asynchronously retrieves the request log from the HTTP response message.
    /// </summary>
    /// <param name="response">The HTTP response message containing the request information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the request log as a string.</returns>
    private async Task<string> GetRequestLogAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
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
                .Any(h => h.Key == _options.CurrentValue.ApiKeyHeaderName
                    && !string.IsNullOrWhiteSpace(h.Value.ToString()));
            stringBuilder.AppendLine($"Contains API Key: {containsApiKeyHeader.ToString()}");
        }

        if (response.RequestMessage?.Content is null)
        {
            return stringBuilder.ToString();
        }

        string requestString = await response.RequestMessage.Content.ReadAsStringAsync(cancellationToken);
        stringBuilder.AppendLine($"Request Body: {requestString}");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Logs an error for an unsuccessful HTTP response status code and returns an error.
    /// </summary>
    /// <param name="response">The HTTP response message containing the error status code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task LogStatusCodeErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        string requestString = await GetRequestLogAsync(response, cancellationToken);

        _logger.LogError(
            "Response status code was not successful: "
            + "\nResponse:"
            + "\t\nStatusCode: {StatusCode}"
            + "\t\nResponse Content: {Response}"
            + "\nRequest:"
            + "\t\n{Request}",
            response.StatusCode,
            responseString,
            requestString);
    }

    /// <summary>
    /// Logs an exception and returns an error indicating a deserialization exception.
    /// </summary>
    /// <param name="exception">The exception that was caught during deserialization.</param>
    /// <param name="type">The type of the response that was being deserialized.</param>
    /// <param name="jsonContent">The JSON content that was being deserialized.</param>
    /// <returns>An <see cref="Error"/> indicating a deserialization exception.</returns>
    private Error LogAndReturnDeserializationException(
        Exception exception,
        Type type,
        string jsonContent)
    {
        _logger.LogError(
            exception,
            "Exception caught while trying to deserialize a response of type {ResponseType} at {ServiceName}.\nJSON Content: {JsonContent}",
            type.Name,
            _options.CurrentValue.ServiceName,
            jsonContent);
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
            _options.CurrentValue.ServiceName);
        return Error.Unexpected("Response.NullDeserialization", "An object was null after deserialization.");
    }
}