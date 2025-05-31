using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Files;
using Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Constants;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Domain;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Objects;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Analyzers.HybridAnalysis.Infrastructure.Factories;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Analyzers.HybridAnalysis.Adapters;

/// <summary>
/// Analyzer implementation for file-based analysis that processes file analysis requests and returns results from sandbox analysis.
/// </summary>
/// <remarks>
/// This class handles the analysis workflow for files, communicating with the sandbox analyzer
/// and managing analysis status and results.
/// </remarks>
internal class FileAnalyzer : Analyzer<FileAnalysis, AnalyzeFileRequest>
{
    private const int InitialSandboxAnalysisStatusCheckDelayMs = 300;

    private readonly IOptionsMonitor<HybridAnalyzerOptions> _analyzerOptions;
    private readonly IMimeTypeDetector _mimeTypeDetector;
    private readonly ISandboxAnalyzer _sandboxAnalyzer;
    private readonly IQuickScanner _quickScanner;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalyzer"/> class.
    /// </summary>
    /// <param name="analyzerOptions">The options monitor for hybrid analyzer configuration.</param>
    /// <param name="rateQuotaService">Service for managing rate limiting quotas.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="logger">Logger for the file analyzer.</param>
    /// <param name="mimeTypeDetector">Service for detecting MIME types of files.</param>
    /// <param name="sandboxAnalyzer">Service for performing sandbox analysis operations.</param>
    /// <param name="quickScanner">Service for performing quick preliminary file scanning.</param>
    public FileAnalyzer(
        IOptionsMonitor<HybridAnalyzerOptions> analyzerOptions,
        [FromKeyedServices(UrlAnalyzer.LimitTrackerServiceKey)] IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        IServiceLogger<FileAnalyzer> logger,
        IMimeTypeDetector mimeTypeDetector,
        ISandboxAnalyzer sandboxAnalyzer,
        IQuickScanner quickScanner)
        : base(analyzerOptions, rateQuotaService, httpClientFactory, logger)
    {
        _analyzerOptions = analyzerOptions;
        _mimeTypeDetector = mimeTypeDetector;
        _sandboxAnalyzer = sandboxAnalyzer;
        _quickScanner = quickScanner;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        CancellationToken cancellationToken = default)
    {
        int fileMaxSize = _analyzerOptions.CurrentValue.FileMaxSizeInBytes;
        if (request.FileSize > fileMaxSize)
        {
            return ServiceErrors.FileTooLarge;
        }

        Stream fileData = await request.StreamFactory.CreateStreamAsync();
        string mimeType = await _mimeTypeDetector.DetectAsync(
            fileData,
            request.FileContentType,
            cancellationToken);

        bool useSandbox = _sandboxAnalyzer.CanAnalyzeMimeType(mimeType);

        ErrorOr<FileAnalysis>? analyzeResult = null;
        if (useSandbox)
        {
            analyzeResult = await AnalyzeWithSandboxAsync(
                httpClient,
                request,
                mimeType,
                cancellationToken);
        }

        if (analyzeResult is { IsError: false })
        {
            return analyzeResult.Value;
        }

        return await AnalyzeWithQuickScanAsync(
            httpClient,
            request,
            mimeType,
            cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ComposedAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var formattedId = FormattedAnalysisId.Parse(id.Primary.Value);
        bool analyzedWithSandbox = formattedId.AnalysisType is AnalysisType.Sandbox;

        return analyzedWithSandbox
            ? await GetSandboxStatusAsync(
                httpClient,
                formattedId,
                cancellationToken)
            : await GetQuickScanStatusAsync(
                httpClient,
                formattedId,
                cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        var formattedId = FormattedAnalysisId.Parse(id.Primary.Value);
        bool analyzedWithSandbox = formattedId.AnalysisType is AnalysisType.Sandbox;

        return analyzedWithSandbox
            ? await GetSandboxAnalysisAsync(
                httpClient,
                formattedId,
                cancellationToken)
            : await GetQuickScanAnalysisAsync(
                httpClient,
                formattedId,
                cancellationToken);
    }

    /// <summary>
    /// Validates that the provided analysis ID is a sandbox analysis ID.
    /// </summary>
    /// <param name="id">The formatted analysis ID to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the analysis type is not a sandbox analysis.</exception>
    private static void ValidateSandboxId(FormattedAnalysisId id)
    {
        if (id.AnalysisType is not AnalysisType.Sandbox)
        {
            throw new InvalidOperationException("The file was not analyzed with sandbox service.");
        }
    }

    /// <summary>
    /// Analyzes a file using the sandbox service for detailed malware analysis.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests to the sandbox service.</param>
    /// <param name="request">The file analysis request containing file data and metadata to analyze.</param>
    /// <param name="mimeType">The MIME type of the file to be analyzed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a <see cref="FileAnalysis"/> with analysis results or error details if the operation fails.</returns>
    private async Task<ErrorOr<FileAnalysis>> AnalyzeWithSandboxAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        _logger.LogDebug("Analyzing file with sandbox service.");
#endif

        SandboxEnvironment osEnvironment =
            _sandboxAnalyzer.DetermineEnvironment(mimeType);
        var requestFactory = new FileSandboxRequestFactory(
            _analyzerOptions.CurrentValue,
            request,
            await request.StreamFactory.CreateStreamAsync(),
            osEnvironment,
            mimeType);
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
        var formattedId = FormattedAnalysisId.Create(
            response.JobId,
            AnalysisType.Sandbox);

        var serviceAnalysis = FileAnalysis.Create(
            formattedId.ToString(),
            ServiceName,
            AnalysisStatus.Queued,
            Verdict.Unknown);

        await Task.Delay(InitialSandboxAnalysisStatusCheckDelayMs, cancellationToken);
        ErrorOr<bool> analysisFailedResult = await HasSandboxAnalysisFailedAsync(
            serviceAnalysis.Id,
            cancellationToken);

        if (analysisFailedResult.IsError)
        {
            return analysisFailedResult.Errors;
        }

        bool hasAnalysisFailed = analysisFailedResult.Value;
        if (hasAnalysisFailed)
        {
            return Error.Failure();
        }

        return serviceAnalysis;
    }

    /// <summary>
    /// Retrieves the current status of a sandbox analysis.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests to the sandbox service.</param>
    /// <param name="id">The raw identifier of the sandbox analysis job.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either an <see cref="AnalysisStatus"/> representing the current status of the analysis or error details if the operation fails.</returns>
    private async Task<ErrorOr<AnalysisStatus>> GetSandboxStatusAsync(
        HttpClient httpClient,
        FormattedAnalysisId id,
        CancellationToken cancellationToken)
    {
        ValidateSandboxId(id);

        ErrorOr<Status> result = await _sandboxAnalyzer.GetReportStatusAsync(
            httpClient,
            id.RawId,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value];
    }

    /// <summary>
    /// Retrieves the analysis results for a file that was processed through the sandbox service.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests to the sandbox service.</param>
    /// <param name="id">The formatted analysis ID containing the job identifier and analysis type.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a <see cref="FileAnalysis"/> with analysis results or error details if the operation fails.</returns>
    private async Task<ErrorOr<FileAnalysis>> GetSandboxAnalysisAsync(
        HttpClient httpClient,
        FormattedAnalysisId id,
        CancellationToken cancellationToken)
    {
        ValidateSandboxId(id);

        ErrorOr<SandboxReportSummary> result = await _sandboxAnalyzer.GetReportSummaryAsync(
            httpClient,
            id.RawId,
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
    /// Checks whether a sandbox analysis has failed or timed out.
    /// </summary>
    /// <param name="id">The composite identifier for the sandbox analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a boolean indicating failure status (true if failed or timed out) or error details if the operation fails.</returns>
    private async Task<ErrorOr<bool>> HasSandboxAnalysisFailedAsync(
        ComposedAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ErrorOr<AnalysisStatus> result = await GetStatusAsync(id, cancellationToken);

        if (result.IsError)
        {
            _logger.LogError("Error when trying to validate status of a sandbox analysis.");
            return result.Errors;
        }

        AnalysisStatus status = result.Value;

        return status is AnalysisStatus.Failed or AnalysisStatus.Timeout;
    }

    /// <summary>
    /// Creates a new file service analysis from a sandbox report summary.
    /// </summary>
    /// <param name="reportSummary">The summary of the sandbox analysis report containing verdict and status information.</param>
    /// <returns>A new <see cref="FileAnalysis"/> instance with information from the report summary.</returns>
    private FileAnalysis CreateAnalysisFromReport(
        SandboxReportSummary reportSummary)
    {
        var formattedId = FormattedAnalysisId.Create(
            reportSummary.JobId,
            AnalysisType.Sandbox);

        AnalysisStatus status = Maps.AnalysisStatusMap[reportSummary.Status];
        Verdict verdict = Maps.VerdictMap[reportSummary.Verdict];
        ThreatScore threatScore = ThreatScore.Create(
            reportSummary.ThreatScore,
            AnalysisSummary.MaxPossibleThreatScore);
        return FileAnalysis.Create(
            formattedId.ToString(),
            ServiceName,
            status,
            verdict,
            threatScore: threatScore);
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

    /// <summary>
    /// Analyzes a file using the quick scan service for rapid preliminary malware detection.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests to the quick scan service.</param>
    /// <param name="request">The file analysis request containing file data and metadata to analyze.</param>
    /// <param name="mimeType">The MIME type of the file to be analyzed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a <see cref="FileAnalysis"/> with analysis results or error details if the operation fails.</returns>
    private async Task<ErrorOr<FileAnalysis>> AnalyzeWithQuickScanAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        _logger.LogDebug("Analyzing file with quick scan.");
#endif

        ErrorOr<QuickScanService> serviceResult =
            await GetBestQuickScanServiceAsync(httpClient, cancellationToken);

        if (serviceResult.IsError)
        {
            return serviceResult.Errors;
        }

        QuickScanService bestQuickScanService = serviceResult.Value;
        var requestFactory = new FileQuickScanRequestFactory(
            request,
            await request.StreamFactory.CreateStreamAsync(),
            bestQuickScanService.Name,
            mimeType);
        ErrorOr<QuickScanResponse> scanResult = await _quickScanner.ScanAsync(
            httpClient,
            requestFactory,
            cancellationToken);

        if (scanResult.IsError)
        {
            return scanResult.Errors;
        }

        QuickScanResponse quickScanResponse = scanResult.Value;
        var formattedId = FormattedAnalysisId.Create(
            quickScanResponse.Id,
            AnalysisType.QuickScan);

        if (quickScanResponse.IsFinished)
        {
            return await GetQuickScanAnalysisAsync(
                httpClient,
                formattedId,
                cancellationToken);
        }

        return FileAnalysis.Create(
            formattedId.ToString(),
            ServiceName,
            AnalysisStatus.Queued,
            Verdict.Unknown);
    }

    /// <summary>
    /// Retrieves analysis results from a completed quick scan.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests to the quick scan service.</param>
    /// <param name="id">The formatted analysis ID for the quick scan.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a <see cref="FileAnalysis"/> with quick scan results or error details if the operation fails.</returns>
    private async Task<ErrorOr<FileAnalysis>> GetQuickScanAnalysisAsync(
        HttpClient httpClient,
        FormattedAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        if (id.AnalysisType is not AnalysisType.QuickScan)
        {
            throw new InvalidOperationException("The file was not analyzed with quick scan service.");
        }

        var quickScanResult = await _quickScanner.GetQuickScanAsync(
            httpClient,
            id.RawId,
            cancellationToken);

        if (quickScanResult.IsError)
        {
            return quickScanResult.Errors;
        }

        QuickScanResponse quickScanResponse = quickScanResult.Value;

        var analysisSummaryResult = await _quickScanner.GetAnalysisSummaryAsync(
            httpClient,
            quickScanResponse.Sha256,
            cancellationToken);

        if (analysisSummaryResult.IsError)
        {
            return analysisSummaryResult.Errors;
        }

        AnalysisStatus status = quickScanResponse.IsFinished
            ? AnalysisStatus.Completed
            : AnalysisStatus.Failed;

        AnalysisSummary analysisSummary = analysisSummaryResult.Value;
        Verdict verdict = Maps.VerdictMap[analysisSummary.Verdict];

#if DEBUG
        DebugAnalysisSummary(analysisSummary);
#endif

        ThreatScore threatScore = ThreatScore.Create(
            analysisSummary.ThreatScore,
            AnalysisSummary.MaxPossibleThreatScore);
        return FileAnalysis.Create(
            id.ToString(),
            ServiceName,
            status,
            verdict,
            threatScore: threatScore);
    }

    /// <summary>
    /// Retrieves the current status of a quick scan analysis.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests to the quick scan service.</param>
    /// <param name="id">The formatted identifier of the quick scan analysis job.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either an <see cref="AnalysisStatus"/> representing the current status of the analysis or error details if the operation fails.</returns>
    private async Task<ErrorOr<AnalysisStatus>> GetQuickScanStatusAsync(
        HttpClient httpClient,
        FormattedAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ErrorOr<FileAnalysis> analysisResult =
            await GetQuickScanAnalysisAsync(httpClient, id, cancellationToken);

        if (analysisResult.IsError)
        {
            return analysisResult.Errors;
        }

        return analysisResult.Value.State.Status;
    }

    /// <summary>
    /// Selects the most suitable quick scan service based on availability and service capabilities.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making API requests to the quick scan service.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either the best available <see cref="QuickScanService"/> or error details if no suitable service is found.</returns>
    private async Task<ErrorOr<QuickScanService>> GetBestQuickScanServiceAsync(
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ErrorOr<QuickScanService[]> stateResult = await _quickScanner.GetStateAsync(
            httpClient,
            cancellationToken);

        if (stateResult.IsError)
        {
            return stateResult.Errors;
        }

        QuickScanService[] availableServices = stateResult.Value
            .Where(s => s.IsAvailable)
            .Where(s => s.SupportedTypes.Contains("file"))
            .ToArray();

        if (availableServices.Length is 0)
        {
            _logger.LogWarning("Quick scan of Hybrid Analysis is unavailable.");
            return Error.Failure("Quick scan is unavailable.");
        }

        return availableServices.Any(IsPreferredScanType)
                ? availableServices.First(IsPreferredScanType)
                : availableServices[0];

        bool IsPreferredScanType(QuickScanService qs) => string.Equals(
            qs.Name,
            _analyzerOptions.CurrentValue.PreferredQuickScanServiceName,
            StringComparison.OrdinalIgnoreCase);
    }

#if DEBUG
    /// <summary>
    /// Logs debug information about the sandbox analysis report summary.
    /// </summary>
    /// <param name="reportSummary">The summary of the sandbox analysis report containing verdict and threat score.</param>
    private void DebugReportSummary(SandboxReportSummary reportSummary)
    {
        _logger.LogDebug(
            "Hybrid Analysis Sandbox File Results:"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreatScore: {ThreatScore}",
            reportSummary.Verdict,
            reportSummary.ThreatScore);
    }

    /// <summary>
    /// Logs debug information about the quick-scan analysis summary.
    /// </summary>
    /// <param name="analysisSummary">The summary of the quick-scan analysis containing verdict and threat score information.</param>
    private void DebugAnalysisSummary(AnalysisSummary analysisSummary)
    {
        _logger.LogDebug(
            "Hybrid Analysis Quick-Scan File Results:"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreatScore: {ThreatScore}",
            analysisSummary.Verdict,
            analysisSummary.ThreatScore);
    }
#endif
}