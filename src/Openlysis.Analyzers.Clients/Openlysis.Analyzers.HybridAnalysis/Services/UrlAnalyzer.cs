using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Analyzers.HybridAnalysis.Services;

/// <summary>
/// Analyzer of URLs.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    /// <summary>
    /// Key of a <see cref="HttpClient"/> service for the <see cref="UrlAnalyzer"/>.
    /// </summary>
    public const string HttpClientServiceKey = "HybridAnalysisHttpClient";

    private readonly IOptionsMonitor<HybridAnalyzerOptions> _hybridOptions;
    private readonly ISandboxAnalyzer _sandboxAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="HybridAnalyzerOptions"/>.</param>
    /// <param name="requestLimitTracker">The request limit tracker.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="sandboxAnalyzer">The sandbox analyzer.</param>
    public UrlAnalyzer(
        IOptionsMonitor<HybridAnalyzerOptions> options,
        IRequestLimitTracker requestLimitTracker,
        [FromKeyedServices(HttpClientServiceKey)] HttpClient httpClient,
        ILogger<UrlAnalyzer> logger,
        ISandboxAnalyzer sandboxAnalyzer)
        : base(options, requestLimitTracker, httpClient, logger)
    {
        _hybridOptions = options;
        _sandboxAnalyzer = sandboxAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(AnalyzeUrlRequest request, CancellationToken cancellationToken = default)
    {
        var submitUrlRequest = new SubmitUrlRequest(
            request.Url,
            _hybridOptions.CurrentValue.DefaultSandboxEnvironment,
            true);
        ErrorOr<SandboxSubmitResponse> result = await _sandboxAnalyzer.AnalyzeAsync(
            _httpClient,
            submitUrlRequest,
            cancellationToken);

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
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(ComposedServiceAnalysisId id, CancellationToken cancellationToken = default)
    {
        ErrorOr<Status> result = await _sandboxAnalyzer.GetReportStatusAsync(
            _httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnGetAnalysisAsync(ComposedServiceAnalysisId id, CancellationToken cancellationToken = default)
    {
        ErrorOr<SanboxReportSummary> result = await _sandboxAnalyzer.GetReportSummaryAsync(
            _httpClient,
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