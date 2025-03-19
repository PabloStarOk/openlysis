using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.Configuration;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Analyzers.Filescan.Core.Models.Objects;
using Openlysis.Analyzers.Filescan.Core.Models.Requests;
using Openlysis.Analyzers.Filescan.Core.Models.Responses;
using Openlysis.Analyzers.Filescan.Infrastructure.Factories;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Analyzers.Filescan.Services;

/// <summary>
/// Represents an analyzer for URLs, inheriting from the base Analyzer class.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    private readonly IFilescanAnalyzer _filescanAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="AnalyzerOptions"/>.</param>
    /// <param name="requestLimitTracker">The request limit tracker for the service.</param>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="logger">The logger for logging information.</param>
    /// <param name="filescanAnalyzer">The filescan analyzer for analyzing files.</param>
    public UrlAnalyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IRequestLimitTracker requestLimitTracker,
        [FromKeyedServices(ServiceConstants.ServiceName)] HttpClient httpClient,
        ILogger<UrlAnalyzer> logger,
        IFilescanAnalyzer filescanAnalyzer)
        : base(options, requestLimitTracker, httpClient, logger)
    {
        _filescanAnalyzer = filescanAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(
        AnalyzeUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var urlRequestFactory = new UrlRequestFactory(request);

        ErrorOr<ScanResponse> result = await _filescanAnalyzer.AnalyzeAsync(
            _httpClient,
            urlRequestFactory,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        ScanResponse scanResponse = result.Value;
        return UrlServiceAnalysis.Create(
            scanResponse.FlowId,
            ServiceName,
            AnalysisStatus.Queued,
            Verdict.Unknown);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(id.Primary.Value);

        ErrorOr<GetAnalysisResponse> result = await _filescanAnalyzer.GetAnalysisAsync(
            _httpClient,
            getScanRequest,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value.Status];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnGetAnalysisAsync(
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(id.Primary.Value);

        ErrorOr<GetAnalysisResponse> result = await _filescanAnalyzer.GetAnalysisAsync(
            _httpClient,
            getScanRequest,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        GetAnalysisResponse analysisResponse = result.Value;

        if (analysisResponse.Reports.Count > 1)
        {
            _logger.LogError(
                "Get analysis response contains more than 1 report at {ServiceName} service analyzer. \nTotal Reports Received: {TotalReportsReceived}",
                ServiceName,
                analysisResponse.Reports.Count);
            return Error.Unexpected("Filescan get analysis response contains more than 1 report.");
        }

        Verdict verdict = Verdict.Unknown;
        float? threatLevel = null;
        if (analysisResponse.Reports.Count is not 0)
        {
            FilescanReport filescanReport = analysisResponse.Reports.First().Value;

            verdict = Maps.VerdictMap[filescanReport.FinalVerdict?.Verdict
                ?? FilescanVerdict.Unknown];
            threatLevel = filescanReport.FinalVerdict?.ThreatLevel;
        }

        AnalysisStatus status = Maps.AnalysisStatusMap[analysisResponse.Status];
        return UrlServiceAnalysis.Create(
            analysisResponse.FlowId,
            ServiceName,
            status,
            verdict,
            threatScore: threatLevel);
    }
}