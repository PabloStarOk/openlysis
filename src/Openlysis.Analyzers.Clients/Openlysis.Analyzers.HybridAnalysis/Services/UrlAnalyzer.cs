using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Analyzers.HybridAnalysis.Infrastructure.Services;
using Openlysis.Analyzers.Shared.Core.Common.Abstractions;
using Openlysis.Analyzers.Shared.Core.URLs.Requests;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.RateQuota.Abstractions;

namespace Openlysis.Analyzers.HybridAnalysis.Services;

/// <summary>
/// Analyzer of URLs.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    /// <summary>
    /// Key of the service for tracking request limits.
    /// </summary>
    public const string LimitTrackerServiceKey = "HybridAnalysisLimitTracker";

    private readonly IOptionsMonitor<HybridAnalyzerOptions> _hybridOptions;
    private readonly ISandboxAnalyzer _sandboxAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="HybridAnalyzerOptions"/>.</param>
    /// <param name="rateQuotaService">The request limit tracker.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="sandboxAnalyzer">The sandbox analyzer.</param>
    public UrlAnalyzer(
        IOptionsMonitor<HybridAnalyzerOptions> options,
        [FromKeyedServices(LimitTrackerServiceKey)] IRateQuotaService rateQuotaService,
        IHttpClientFactory httpClientFactory,
        SandboxAnalyzerLogger logger,
        ISandboxAnalyzer sandboxAnalyzer)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _hybridOptions = options;
        _sandboxAnalyzer = sandboxAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var submitUrlRequest = new SubmitUrlRequest(
            request.Url,
            _hybridOptions.CurrentValue.DefaultSandboxEnvironment,
            true);
        ErrorOr<SandboxSubmitResponse> result = await _sandboxAnalyzer.AnalyzeAsync(
            httpClient,
            submitUrlRequest,
            cancellationToken);

        if (result.Errors.Any(e => e.Code is ErrorCodes.TooManyRequests))
        {
            // Get last analysis report.
            ErrorOr<string> checkHashResult = await _sandboxAnalyzer
                .CheckUrlHashAsync(httpClient, request.Url, cancellationToken);
            if (checkHashResult.IsError)
            {
                return checkHashResult.Errors;
            }

            string hash = checkHashResult.Value;
            int sandboxEnvironmentId = (int)_hybridOptions.CurrentValue.DefaultSandboxEnvironment;
            string id = $"{hash}:{sandboxEnvironmentId}";

            ErrorOr<SanboxReportSummary> reportResult =
                await _sandboxAnalyzer.GetReportSummaryAsync(httpClient, id, cancellationToken);

            if (reportResult.IsError)
            {
                return reportResult.Errors;
            }

            return MapServiceAnalysis(reportResult.Value);
        }

        if (result.IsError)
        {
            return result.Errors;
        }

        SandboxSubmitResponse response = result.Value;
        return UrlServiceAnalysis.Create(
            response.JobId,
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
        ErrorOr<Status> result = await _sandboxAnalyzer.GetReportStatusAsync(
            httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ErrorOr<SanboxReportSummary> result = await _sandboxAnalyzer.GetReportSummaryAsync(
            httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        SanboxReportSummary reportSummary = result.Value;

        if (reportSummary.Status is Status.Error)
        {
            return Error.Failure($"Status of analysis is Error and analysis information cannot be retrieved. {ServiceName} service analyzer.");
        }

#if DEBUG
        _logger.LogDebug(
            "Hybrid Analysis Results:"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreatScore: {ThreatScore}",
            reportSummary.Verdict,
            reportSummary.ThreatScore);
#endif

        return MapServiceAnalysis(reportSummary);
    }

    /// <summary>
    /// Maps from <see cref="SanboxReportSummary"/> to an <see cref="UrlServiceAnalysis"/> object.
    /// </summary>
    /// <param name="reportSummary">The summary of the sandbox report.</param>
    /// <returns>A UrlServiceAnalysis object containing the mapped data.</returns>
    private UrlServiceAnalysis MapServiceAnalysis(SanboxReportSummary reportSummary)
    {
        AnalysisStatus status = Maps.AnalysisStatusMap[reportSummary.Status];
        Verdict verdict = Maps.VerdictMap[reportSummary.Verdict];
        return UrlServiceAnalysis.Create(
            reportSummary.JobId,
            ServiceName,
            status,
            verdict,
            threatScore: reportSummary.ThreatScore);
    }
}