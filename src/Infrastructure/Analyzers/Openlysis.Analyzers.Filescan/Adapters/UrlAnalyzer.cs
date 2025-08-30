using System.Text;

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
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Analyzers.Filescan.Adapters;

/// <summary>
/// Represents an analyzer for URLs, inheriting from the base Analyzer class.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlAnalysis, AnalyzeUrlRequest>
{
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
        [FromKeyedServices(KeyedServices.GlobalKey)] IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceLogger<UrlAnalyzer> logger,
        IFilescanAnalyzer filescanAnalyzer)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _filescanAnalyzer = filescanAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlAnalysis>> OnAnalyzeAsync(
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
        return UrlAnalysis.Create(
            scanResponse.FlowId,
            ServiceName,
            AnalysisStatus.Queued,
            Verdict.Unknown);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ExternalAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(id.Primary);

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
    protected override async Task<ErrorOr<UrlAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ExternalAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(id.Primary);

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

#if DEBUG
        DebugAnalysis(analysisResponse);
#endif

        Verdict verdict = Verdict.Unknown;
        ThreatScore threatScore = ThreatScore.Create(null, null);
        if (analysisResponse.Reports.Count is not 0)
        {
            FilescanReport filescanReport = analysisResponse.Reports.First().Value;

            verdict = Maps.VerdictMap[filescanReport.FinalVerdict?.Verdict
                ?? FilescanVerdict.Unknown];
            threatScore = ThreatScore.Create(
                filescanReport.FinalVerdict?.ThreatLevel,
                FinalVerdict.MaxPossibleThreatLevel);
        }

        AnalysisStatus status = Maps.AnalysisStatusMap[analysisResponse.Status];
        return UrlAnalysis.Create(
            analysisResponse.FlowId,
            ServiceName,
            status,
            verdict,
            threatScore: threatScore);
    }

#if DEBUG
    private void DebugAnalysis(GetAnalysisResponse response)
    {
        var stringBuilder = new StringBuilder();
        foreach ((string id, FilescanReport report) in response.Reports)
        {
            stringBuilder.AppendLine(
                "Filescan URL Report:"
                + $"\n\tID: {id}"
                + $"\n\tVerdict: {report.FinalVerdict?.Verdict}"
                + $"\n\tThreatScore: {report.FinalVerdict?.ThreatLevel}");
        }

        _logger.LogTrace(
            "Filescan analysis results:"
            + "\n\tFlow ID: {FlowId}"
            + "\n\tStatus: {Status}"
            + "\n\tReports: {Reports}",
            response.FlowId,
            response.Status,
            stringBuilder.ToString());
    }
#endif
}