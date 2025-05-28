using System.Net;
using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Analyzers.HybridAnalysis.Infrastructure.Logging;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Analyzers;

/// <summary>
/// Represents an analyzer which sends request to Hybrid Analysis to analyze Files or URLs.
/// </summary>
internal class SandboxAnalyzer : ISandboxAnalyzer
{
    /// <summary>
    /// This constant is used to identify and retrieve services that are keyed for the Hybrid Analysis sandbox analyzer.
    /// </summary>
    public const string KeyedServicesKey = "HybridAnalysisServices";

    private readonly IOptionsMonitor<SandboxAnalyzerOptions> _options;
    private readonly SandboxAnalyzerLogger<SandboxAnalyzer> _analyzerLogger;
    private readonly IServiceDeserializer _serviceDeserializer;
    private readonly IReadOnlyList<MimeEnvironmentMapping> _environmentMappings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for accessing configuration settings for the analyzer.</param>
    /// <param name="analyzerLogger">The logger instance to log analyzer activities.</param>
    /// <param name="serviceDeserializer">The deserializer instance to handle response deserialization.</param>
    public SandboxAnalyzer(
        IOptionsMonitor<SandboxAnalyzerOptions> options,
        SandboxAnalyzerLogger<SandboxAnalyzer> analyzerLogger,
        [FromKeyedServices(KeyedServicesKey)] IServiceDeserializer serviceDeserializer)
    {
        _options = options;
        _analyzerLogger = analyzerLogger;
        _serviceDeserializer = serviceDeserializer;

        _environmentMappings =
        [
            new MimeEnvironmentMapping(
                _options.CurrentValue.AnyOsSupportedMimeTypes,
                SandboxEnvironment.Windows11X64),
            new MimeEnvironmentMapping(
                _options.CurrentValue.WinOnlySupportedMimeTypes,
                SandboxEnvironment.Windows11X64),
            new MimeEnvironmentMapping(
                _options.CurrentValue.Win7HwpOnlySupportedMimeTypes,
                SandboxEnvironment.Windows7X32HwpSupport),
            new MimeEnvironmentMapping(
                _options.CurrentValue.LinuxOnlySupportedMimeTypes,
                SandboxEnvironment.LinuxUbuntuX64),
            new MimeEnvironmentMapping(
                _options.CurrentValue.MacOnlySupportedMimeTypes,
                SandboxEnvironment.MacCatalinaX64),
            new MimeEnvironmentMapping(
                _options.CurrentValue.AndroidOnlySupportedMimeTypes,
                SandboxEnvironment.AndroidStaticAnalysis)
        ];
    }

    /// <inheritdoc/>
    public bool CanAnalyzeMimeType(string mimeType)
    {
        return _options.CurrentValue.AllSupportedFiles
            .Contains(mimeType, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public SandboxEnvironment DetermineEnvironment(string mimeType)
    {
        SandboxEnvironment? environment = null;

        foreach (var mapping in _environmentMappings)
        {
            if (!mapping.SupportMimeType(mimeType))
            {
                continue;
            }

            environment = mapping.SandboxEnvironment;
            break;
        }

        if (environment is null)
        {
            throw new NotSupportedException(
                $"MIME type '{mimeType}' is not supported by any sandbox environment.");
        }

        return (SandboxEnvironment)environment;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<SandboxSubmitResponse>> AnalyzeAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default)
    {
        using HybridAnalysisAnalyzeRequest request = requestFactory.Create();
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

    /// <summary>
    /// Represents a mapping between supported MIME types and a sandbox environment.
    /// </summary>
    /// <param name="SupportedMimeTypes">The set of MIME types supported by the environment.</param>
    /// <param name="SandboxEnvironment">The sandbox environment associated with the MIME types.</param>
    private sealed record MimeEnvironmentMapping(
        IReadOnlySet<string> SupportedMimeTypes,
        SandboxEnvironment SandboxEnvironment)
    {
        /// <summary>
        /// Determines whether the specified MIME type is supported by this environment mapping.
        /// </summary>
        /// <param name="mimeType">The MIME type to check.</param>
        /// <returns><c>true</c> if the MIME type is supported; otherwise, <c>false</c>.</returns>
        internal bool SupportMimeType(string mimeType)
            => SupportedMimeTypes
                .Select(f => f.Trim())
                .Contains(mimeType, StringComparer.OrdinalIgnoreCase);
    }
}