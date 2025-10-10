using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Models;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Constants;
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;
using Openlysis.Analyzers.URLQuery.Core.Models.Requests;
using Openlysis.Analyzers.URLQuery.Core.Models.Responses;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.Deserialization.Abstractions;

namespace Openlysis.Analyzers.URLQuery.Adapters;

/// <summary>
/// Analyzer of URLs using the 'urlquery.net' service.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlAnalysis, AnalyzeUrlRequest>
{
    private readonly IOptionsMonitor<UrlQueryAnalyzerOptions> _urlQueryOptions;
    private readonly IVerdictCalculator _verdictCalculator;
    private readonly IServiceDeserializer _serviceDeserializer;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="UrlQueryAnalyzerOptions"/>.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="verdictCalculator">The calculator for determining the verdict of the URL analysis.</param>
    /// <param name="logger">The logger for logging analyzer-specific information.</param>
    /// <param name="serviceDeserializer">The deserializer for analyzing responses.</param>
    public UrlAnalyzer(
        IOptionsMonitor<UrlQueryAnalyzerOptions> options,
        IHttpClientFactory httpClientFactory,
        IVerdictCalculator verdictCalculator,
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceLogger<UrlAnalyzer> logger,
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceDeserializer serviceDeserializer)
        : base(options, httpClientFactory, logger)
    {
        _urlQueryOptions = options;
        _verdictCalculator = verdictCalculator;
        _serviceDeserializer = serviceDeserializer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlAnalysis>> OnAnalyzeAsync(
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
            await _logger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        ErrorOr<SubmitUrlResponse> result = await _serviceDeserializer
            .DeserializeAsync<SubmitUrlResponse>(response, cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        // Return analysis
        SubmitUrlResponse submitUrlResponse = result.Value;
        AnalysisStatus status = Maps.AnalysisStatusMap[submitUrlResponse.Status];
        return UrlAnalysis.Create(
            submitUrlResponse.ReportId,
            ServiceName,
            status,
            Verdict.Unknown,
            submitUrlResponse.QueueId);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ExternalAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id.Job);

        string formattedUrl = string.Format(Addresses.SubmitStatusEndpoint, id.Job);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _logger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        ErrorOr<SubmitUrlResponse> result = await _serviceDeserializer
            .DeserializeAsync<SubmitUrlResponse>(response, cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value.Status];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        AnalysisIdentity identity,
        CancellationToken cancellationToken = default)
    {
        // Ensure analysis can be retrieved.
        var statusResult = await OnGetStatusAsync(httpClient, identity.ExternalId, cancellationToken);
        if (statusResult.IsError)
        {
            return statusResult.Errors;
        }

        if (statusResult.Value is AnalysisStatus.Queued or AnalysisStatus.InProgress)
        {
            return UrlAnalysis.CreateWithId(identity.Id, identity.ExternalId, statusResult.Value, Verdict.Unknown);
        }

        // Retrieve analysis.
        string formattedUrl = string.Format(Addresses.ReportEndpoint, identity.ExternalId.Primary);
        using HttpResponseMessage response = await httpClient.GetAsync(formattedUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await _logger.LogNonSuccessStatusCodeAsync(response, cancellationToken);
            return ServiceErrors.NonSuccessStatusCode;
        }

        ErrorOr<GetReportResponse> reportResult = await _serviceDeserializer
            .DeserializeAsync<GetReportResponse>(response, cancellationToken);

        if (reportResult.IsError)
        {
            return reportResult.Errors;
        }

        GetReportResponse report = reportResult.Value;

#if DEBUG
        DebugReport(report);
#endif

        AnalysisStatus status = Maps.AnalysisStatusMap[report.Status];
        Verdict verdict = _verdictCalculator.Calculate(report.Sensors);
        return UrlAnalysis.CreateWithId(identity.Id, identity.ExternalId, status, verdict);
    }

#if DEBUG
    private void DebugReport(GetReportResponse report)
    {
        _logger.LogTrace(
            "urlquery URL report results:"
            + "\n\tReport ID: {ReportId}"
            + "\n\tStatus: {Status}"
            + "\n\tSensors: {Sensors}",
            report.ReportId,
            report.Status,
            report.Sensors);
    }
#endif
}