using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Configuration;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Analyzers.Filescan.Core.Models.Objects;
using Openlysis.Analyzers.Filescan.Core.Models.Requests;
using Openlysis.Analyzers.Filescan.Core.Models.Responses;
using Openlysis.Analyzers.Filescan.Infrastructure.Factories;
using Openlysis.Analyzers.Filescan.Infrastructure.Services;
using Openlysis.Analyzers.Shared.Core.Common.Abstractions;
using Openlysis.Analyzers.Shared.Core.URLs.Requests;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Logging.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;
using Openlysis.Infrastructure.Shared.RateQuota.Enums;

namespace Openlysis.Analyzers.Filescan.Services;

/// <summary>
/// Represents an analyzer for URLs, inheriting from the base Analyzer class.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    /// <summary>
    /// Key of the service for tracking request limits.
    /// </summary>
    public const string LimitTrackerServiceKey = "FilescanLimitTracker";

    private readonly IFilescanAnalyzer _filescanAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="FilescanAnalyzerOptions"/>.</param>
    /// <param name="rateQuotaService">The request limit tracker for the service.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="logger">The logger for logging information.</param>
    /// <param name="filescanAnalyzer">The filescan analyzer for analyzing files.</param>
    public UrlAnalyzer(
        IOptionsMonitor<FilescanAnalyzerOptions> options,
        [FromKeyedServices(LimitTrackerServiceKey)] IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(FilescanAnalyzer.KeyedServicesKey)] IServiceLogger<UrlAnalyzer> logger,
        IFilescanAnalyzer filescanAnalyzer)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _filescanAnalyzer = filescanAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var urlRequestFactory = new UrlRequestFactory(request);

        ErrorOr<ScanResponse> result = await _filescanAnalyzer.AnalyzeAsync(
            httpClient,
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
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(id.Primary.Value);

        ErrorOr<GetAnalysisResponse> result = await _filescanAnalyzer.GetAnalysisAsync(
            httpClient,
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
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(id.Primary.Value);

        ErrorOr<GetAnalysisResponse> result = await _filescanAnalyzer.GetAnalysisAsync(
            httpClient,
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

#if DEBUG
            _logger.LogDebug(
                "Filescan Results:"
                + "\n\tVerdict: {Verdict}"
                + "\n\tThreatScore: {ThreatScore}",
                filescanReport.FinalVerdict?.Verdict,
                filescanReport.FinalVerdict?.ThreatLevel);
#endif
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