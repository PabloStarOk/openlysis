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
public class FileAnalyzer : Analyzer<FileServiceAnalysis, AnalyzeFileRequest>
{
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
        _filescanAnalyzer = filescanAnalyzer;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileServiceAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        CancellationToken cancellationToken = default)
    {
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

        return FileServiceAnalysis.Create(
            result.Value.FlowId,
            ServiceName,
            AnalysisStatus.Queued);
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

        GetAnalysisResponse analysisResponse = result.Value;
        return Maps.AnalysisStatusMap[analysisResponse.Status];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileServiceAnalysis>> OnGetAnalysisAsync(
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

        AnalysisStatus status = Maps.AnalysisStatusMap[analysisResponse.Status];
        var serviceAnalysis = FileServiceAnalysis.Create(
            analysisResponse.FlowId,
            ServiceName,
            status);

        if (analysisResponse.Reports.Count is 0)
        {
            return serviceAnalysis;
        }

        var filescanReportId = analysisResponse.Reports.First().Key;
        var filescanReport = analysisResponse.Reports.First().Value;

#if DEBUG
        _logger.LogDebug(
            "Filescan Results:"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreatScore: {ThreatScore}",
            filescanReport.FinalVerdict?.Verdict,
            filescanReport.FinalVerdict?.ThreatLevel);
#endif

        Verdict verdict = Maps.VerdictMap[filescanReport.FinalVerdict?.Verdict ?? FilescanVerdict.Unknown];
        ThreatZone threatZone = ThreatZoneMapping.Map[verdict];

        var report = Report.Create(
            filescanReportId,
            verdict,
            threatZone,
            filescanReport.FinalVerdict?.ThreatLevel);
        serviceAnalysis.AddReport(report);

        return serviceAnalysis;
    }
}