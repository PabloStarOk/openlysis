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
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Analyzers.Filescan.Adapters;

/// <summary>
/// Represents a file analyzer that performs analysis on files using the Filescan service.
/// </summary>
/// <remarks>
/// This class extends the <see cref="Analyzer{TAnalysis, TRequest}"/> base class,
/// providing specific implementations for analyzing files, retrieving analysis status,
/// and fetching analysis results.
/// </remarks>
public class FileAnalyzer : Analyzer<FileAnalysis, AnalyzeFileRequest>
{
    private readonly IOptionsMonitor<FilescanAnalyzerOptions> _analyzerOptions;
    private readonly IFilescanAnalyzer _filescanAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="FilescanAnalyzerOptions"/>.</param>
    /// <param name="rateQuotaService">
    /// The rate quota service for managing request quotas, injected using a keyed service.
    /// </param>
    /// <param name="httpClientFactory">The factory for creating <see cref="HttpClient"/> instances.</param>
    /// <param name="logger">The logger instance for logging, injected using a keyed service.</param>
    /// <param name="filescanAnalyzer">The filescan analyzer responsible for performing file analysis.</param>
    public FileAnalyzer(
        IOptionsMonitor<FilescanAnalyzerOptions> options,
        [FromKeyedServices(KeyedServices.GlobalKey)] IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(KeyedServices.GlobalKey)] IServiceLogger<FileAnalyzer> logger,
        IFilescanAnalyzer filescanAnalyzer)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _analyzerOptions = options;
        _filescanAnalyzer = filescanAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        CancellationToken cancellationToken = default)
    {
        int fileMaxSize = _analyzerOptions.CurrentValue.FileMaxSizeInBytes;
        if (request.FileData.Length > fileMaxSize)
        {
            return ServiceErrors.FileTooLarge;
        }

        var options = ScanOptions.True;
        var scanRequest = new ScanRequest(
            request.FileName,
            request.FileContentType,
            request.FileData,
            Password: request.FilePassword,
            IsPrivateFile: request.IsPrivateFile,
            Options: options);

        var factory = new FileRequestFactory(scanRequest);
        ErrorOr<ScanResponse> result = await _filescanAnalyzer.AnalyzeAsync(
            httpClient,
            factory,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return FileAnalysis.Create(
            result.Value.FlowId,
            ServiceName,
            AnalysisStatus.Queued,
            Verdict.Unknown);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ComposedAnalysisId id,
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
        return Maps.AnalysisStatusMap[analysisResponse.Status];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedAnalysisId id,
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

#if DEBUG
        DebugFilescanAnalysis(analysisResponse);
#endif

        return CreateAnalysisFromResponse(analysisResponse);
    }

    /// <summary>
    /// Creates a <see cref="FileAnalysis"/> instance from the Filescan analysis response.
    /// </summary>
    /// <param name="response">The analysis response containing reports from the Filescan service.</param>
    /// <returns>A populated <see cref="FileAnalysis"/> object with status and reports extracted from the response.</returns>
    private FileAnalysis CreateAnalysisFromResponse(GetAnalysisResponse response)
    {
        var serviceAnalysis = FileAnalysis.Create(
            response.FlowId,
            ServiceName,
            AnalysisStatus.Queued,
            Verdict.Unknown);

        foreach (var reportKeyValuePair in response.Reports)
        {
            string filescanReportId = reportKeyValuePair.Key;
            FilescanReport filescanReport = reportKeyValuePair.Value;
            FilescanVerdict filescanVerdict =
                filescanReport.FinalVerdict?.Verdict ?? FilescanVerdict.Unknown;

            Verdict verdict = Maps.VerdictMap[filescanVerdict];
            ThreatZone threatZone = ThreatZoneMapping.Map[verdict];
            ThreatScore threatScore = ThreatScore.Create(
                filescanReport.FinalVerdict?.ThreatLevel,
                FinalVerdict.MaxPossibleThreatLevel);
            var report = FileReport.Create(
                filescanReportId,
                verdict,
                threatZone,
                threatScore);
            serviceAnalysis.AddReport(report);
        }

        AnalysisStatus status = Maps.AnalysisStatusMap[response.Status];
        serviceAnalysis.UpdateStatus(status);
        return serviceAnalysis;
    }

#if DEBUG
    /// <summary>
    /// Logs debug information about an analysis of Filescan service.
    /// </summary>
    /// <param name="response">The analysis response containing reports to log.</param>
    private void DebugFilescanAnalysis(GetAnalysisResponse response)
    {
        foreach (var reportKeyValue in response.Reports)
        {
            DebugFilescanReport(
                id: reportKeyValue.Key,
                filescanReport: reportKeyValue.Value);
        }
    }

    /// <summary>
    /// Logs debug information about a specific Filescan report.
    /// </summary>
    /// <param name="id">The unique identifier of the report.</param>
    /// <param name="filescanReport">The Filescan report containing analysis results to log.</param>
    private void DebugFilescanReport(
        string id,
        FilescanReport filescanReport)
    {
        _logger.LogDebug(
            "Filescan Report:"
            + "\n\tID: {ReportId}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreatScore: {ThreatScore}",
            id,
            filescanReport.FinalVerdict?.Verdict,
            filescanReport.FinalVerdict?.ThreatLevel);
    }
#endif
}