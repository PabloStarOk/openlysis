using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Analyzers.HybridAnalysis.Infrastructure.Factories;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Analyzers.HybridAnalysis.Adapters;

/// <summary>
/// Analyzer implementation for file-based analysis that processes file analysis requests and returns results from sandbox analysis.
/// </summary>
/// <remarks>
/// This class handles the analysis workflow for files, communicating with the sandbox analyzer
/// and managing analysis status and results.
/// </remarks>
internal class FileAnalyzer : Analyzer<FileServiceAnalysis, AnalyzeFileRequest>
{
    private readonly ISandboxAnalyzer _sandboxAnalyzer;
    private readonly IOptionsMonitor<HybridAnalyzerOptions> _analyzerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalyzer"/> class.
    /// </summary>
    /// <param name="analyzerOptions">The options monitor for hybrid analyzer configuration.</param>
    /// <param name="rateQuotaService">Service for managing rate limiting quotas.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="logger">Logger for the file analyzer.</param>
    /// <param name="sandboxAnalyzer">Service for performing sandbox analysis operations.</param>
    public FileAnalyzer(
        IOptionsMonitor<HybridAnalyzerOptions> analyzerOptions,
        [FromKeyedServices(UrlAnalyzer.LimitTrackerServiceKey)] IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        IServiceLogger<FileAnalyzer> logger,
        ISandboxAnalyzer sandboxAnalyzer)
        : base(analyzerOptions, rateQuotaService, httpClientFactory, logger)
    {
        _analyzerOptions = analyzerOptions;
        _sandboxAnalyzer = sandboxAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileServiceAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestFactory = new FileRequestFactory(
            _analyzerOptions.CurrentValue,
            request);
        ErrorOr<SandboxSubmitResponse> result = await _sandboxAnalyzer.AnalyzeAsync(
            httpClient,
            requestFactory,
            cancellationToken);

        if (result.Errors.Any(e => e.Code is ErrorCodes.TooManyRequests))
        {
            // Get last analysis report.
            var existingReportResult = await TryGetExistingReportAsync(
                httpClient,
                request,
                cancellationToken);

            if (existingReportResult.IsError)
            {
                return existingReportResult.Errors;
            }

            return CreateAnalysisFromReport(existingReportResult.Value);
        }

        if (result.IsError)
        {
            return result.Errors;
        }

        SandboxSubmitResponse response = result.Value;
        return FileServiceAnalysis.Create(
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
    protected override async Task<ErrorOr<FileServiceAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ErrorOr<SandboxReportSummary> result = await _sandboxAnalyzer.GetReportSummaryAsync(
            httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        SandboxReportSummary reportSummary = result.Value;

        if (reportSummary.Status is Status.Error)
        {
            return Error.Failure($"Status of analysis is Error and analysis information cannot be retrieved. {ServiceName} service analyzer.");
        }

#if DEBUG
        DebugReportSummary(reportSummary);
#endif

        return CreateAnalysisFromReport(reportSummary);
    }

    /// <summary>
    /// Creates a new file service analysis from a sandbox report summary.
    /// </summary>
    /// <param name="reportSummary">The summary of the sandbox analysis report containing verdict and status information.</param>
    /// <returns>A new <see cref="FileServiceAnalysis"/> instance with information from the report summary.</returns>
    private FileServiceAnalysis CreateAnalysisFromReport(
        SandboxReportSummary reportSummary)
    {
        AnalysisStatus status = Maps.AnalysisStatusMap[reportSummary.Status];
        Verdict verdict = Maps.VerdictMap[reportSummary.Verdict];
        return FileServiceAnalysis.Create(
            reportSummary.JobId,
            ServiceName,
            status,
            verdict);
    }

    /// <summary>
    /// Tries to retrieve an existing sandbox analysis report for a file.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make the API request.</param>
    /// <param name="request">The file analysis request containing details of the file to analyze.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a <see cref="SandboxReportSummary"/> if successful or error details if the operation fails.</returns>
    private async Task<ErrorOr<SandboxReportSummary>> TryGetExistingReportAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        CancellationToken cancellationToken = default)
    {
        int sandboxEnvironmentId =
            (int)_analyzerOptions.CurrentValue.DefaultSandboxEnvironment;
        string id = $"{request.FileSha256}:{sandboxEnvironmentId}";

        return await _sandboxAnalyzer
            .GetReportSummaryAsync(httpClient, id, cancellationToken);
    }

#if DEBUG
    /// <summary>
    /// Logs debug information about the sandbox analysis report summary.
    /// </summary>
    /// <param name="reportSummary">The summary of the sandbox analysis report containing verdict and threat score.</param>
    private void DebugReportSummary(
        SandboxReportSummary reportSummary)
    {
        _logger.LogDebug(
            "Hybrid Analysis Results:"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreatScore: {ThreatScore}",
            reportSummary.Verdict,
            reportSummary.ThreatScore);
    }
#endif
}