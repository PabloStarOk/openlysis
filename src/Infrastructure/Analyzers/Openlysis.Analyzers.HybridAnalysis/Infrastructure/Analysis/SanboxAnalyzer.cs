using System.Net;
using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
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
internal class SandboxAnalyzer : ISandboxAnalyzer
{
    /// <summary>
    /// This constant is used to identify and retrieve services that are keyed for the Hybrid Analysis sandbox analyzer.
    /// </summary>
    public const string KeyedServicesKey = "HybridAnalysisServices";

    private readonly SandboxAnalyzerLogger<SandboxAnalyzer> _analyzerLogger;
    private readonly IServiceDeserializer _serviceDeserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxAnalyzer"/> class.
    /// </summary>
    /// <param name="analyzerLogger">The logger instance to log analyzer activities.</param>
    /// <param name="serviceDeserializer">The deserializer instance to handle response deserialization.</param>
    public SandboxAnalyzer(
        SandboxAnalyzerLogger<SandboxAnalyzer> analyzerLogger,
        [FromKeyedServices(KeyedServicesKey)] IServiceDeserializer serviceDeserializer)
    {
        _analyzerLogger = analyzerLogger;
        _serviceDeserializer = serviceDeserializer;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<SandboxSubmitResponse>> AnalyzeAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default)
    {
        using HybridAnalysisSubmitRequest request = requestFactory.Create();
        using HttpResponseMessage response = await httpClient.PostAsync(
            request.EndpointAddress,
            request.HttpContent,
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
    public async Task<ErrorOr<SandboxReportSummary>> GetReportSummaryAsync(
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
            .DeserializeAsync<SandboxReportSummary>(response, cancellationToken);
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
}