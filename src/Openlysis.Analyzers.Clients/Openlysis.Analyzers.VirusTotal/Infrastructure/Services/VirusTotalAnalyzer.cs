using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Configuration;
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
    private readonly ILogger<VirusTotalAnalyzer> _logger;
    private readonly IOptionsMonitor<VirusTotalAnalyzerOptions> _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter<Status>(JsonNamingPolicy.KebabCaseLower),
        },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="VirusTotalAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options monitor for accessing configuration settings.</param>
    public VirusTotalAnalyzer(
        ILogger<VirusTotalAnalyzer> logger,
        IOptionsMonitor<VirusTotalAnalyzerOptions> options)
    {
        _logger = logger;
        _options = options;
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
            return await LogAndReturnStatusCodeErrorAsync(response, cancellationToken);
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
            return await LogAndReturnStatusCodeErrorAsync(response, cancellationToken);
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
    /// <returns>An <see cref="Error"/> indicating the unsuccessful status code.</returns>
    private async Task<Error> LogAndReturnStatusCodeErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
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
        return Error.Unexpected("Response.NotSuccessful", "Response status code was not successful.");
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