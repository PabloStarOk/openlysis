using System.Net;
using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Analyzers.HybridAnalysis.Infrastructure.Logging;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Analysis;

/// <summary>
/// Represents an analyzer which sends request to Hybrid Analysis to analyze Files or URLs.
/// </summary>
public class SandboxAnalyzer : ISandboxAnalyzer
{
    /// <summary>
    /// This constant is used to identify and retrieve services that are keyed for the Hybrid Analysis sandbox analyzer.
    /// </summary>
    public const string KeyedServicesKey = "HybridAnalysisServices";

    private readonly IOptionsMonitor<HybridAnalyzerOptions> _options;
    private readonly SandboxAnalyzerLogger<SandboxAnalyzer> _analyzerLogger;
    private readonly IServiceDeserializer _serviceDeserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor instance to access configuration settings.</param>
    /// <param name="analyzerLogger">The logger instance to log analyzer activities.</param>
    /// <param name="serviceDeserializer">The deserializer instance to handle response deserialization.</param>
    public SandboxAnalyzer(
        IOptionsMonitor<HybridAnalyzerOptions> options,
        SandboxAnalyzerLogger<SandboxAnalyzer> analyzerLogger,
        [FromKeyedServices(KeyedServicesKey)] IServiceDeserializer serviceDeserializer)
    {
        _options = options;
        _analyzerLogger = analyzerLogger;
        _serviceDeserializer = serviceDeserializer;
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

        if (response.IsSuccessStatusCode)
        {
            return await _serviceDeserializer
                .DeserializeAsync<SandboxSubmitResponse>(response, cancellationToken);
        }

        await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
        return response.StatusCode is HttpStatusCode.TooManyRequests
            ? Error.Failure(code: ErrorCodes.TooManyRequests)
            : ServiceErrors.NonSuccessStatusCode;
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
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        ErrorOr<ReportStateResponse> result = await _serviceDeserializer
            .DeserializeAsync<ReportStateResponse>(response, cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        ReportStateResponse reportState = result.Value;
        if (reportState.Status is Status.Error)
        {
            await _analyzerLogger.LogStateErrorAsync(response, reportState, cancellationToken);
        }

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
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        return await _serviceDeserializer
            .DeserializeAsync<SanboxReportSummary>(response, cancellationToken);
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
            await _analyzerLogger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
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
}