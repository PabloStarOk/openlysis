using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Analyzers.Shared.Core.Files.Requests;
using Openlysis.Analyzers.Shared.Interfaces;
using Openlysis.Analyzers.Filescan.Core.Abstractions;
using Openlysis.Analyzers.Filescan.Core.Constants;
using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Analyzers.Filescan.Core.Models.Requests;
using Openlysis.Analyzers.Filescan.Core.Models.Responses;
using Openlysis.Analyzers.Filescan.Core.Models.Scans;
using Openlysis.Analyzers.Filescan.Infrastructure.Factories;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.FileAnalyses.Entities;

namespace Openlysis.Analyzers.Filescan.Services;

/// <summary>
/// Represents a file scanner analyzer that implements the <see cref="IServiceAnalyzer{TAnalysis,TAnalysisId}"/> interface.
/// </summary>
public class FileAnalyzer : IServiceAnalyzer<ServiceFileAnalysis, ServiceAnalysisId>
{
    /// <inheritdoc/>
    public string ServiceName => ServiceConstants.ServiceName;

    private readonly ILogger<FileAnalyzer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFilescanAnalyzer _filescanAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">The logger to be used for logging information.</param>
    /// <param name="httpClientFactory">The HTTP client factory to be used for creating HTTP clients.</param>
    /// <param name="filescanAnalyzer">The file scanner service to be used for file analysis.</param>
    public FileAnalyzer(
        ILogger<FileAnalyzer> logger,
        IHttpClientFactory httpClientFactory,
        IFilescanAnalyzer filescanAnalyzer)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _filescanAnalyzer = filescanAnalyzer;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ServiceAnalysisId>> AnalyzeAsync(FileAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var options = ScanOptions.True;
        var scanRequest = new ScanRequest(
            request.FileName,
            request.FileContentType,
            request.FileData,
            request.FileDescription,
            Password: request.FilePassword,
            IsPrivateFile: request.IsPrivateFile,
            Options: options);

        var factory = new FileRequestFactory(scanRequest);
        var httpClient = _httpClientFactory.CreateClient(ServiceName);
        ErrorOr<ScanResponse> result = await _filescanAnalyzer.AnalyzeAsync(
            httpClient,
            factory,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return ServiceAnalysisId.Create(result.Value.FlowId);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<ServiceFileAnalysis>> GetAnalysisAsync(ServiceAnalysisId analysisId, CancellationToken cancellationToken = default)
    {
        var getScanRequest = new GetScanRequest(analysisId.Value);

        var httpClient = _httpClientFactory.CreateClient(ServiceName);
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
        var serviceAnalysis = ServiceFileAnalysis.Create(
            analysisResponse.FlowId,
            ServiceName,
            status);

        if (analysisResponse.Reports.Count is 0)
        {
            return serviceAnalysis;
        }

        var filescanReportId = analysisResponse.Reports.First().Key;
        var filescanReport = analysisResponse.Reports.First().Value;
        Verdict verdict = Maps.VerdictMap[filescanReport.FinalVerdict?.Verdict ?? FilescanVerdict.Unknown];
        var threatZone = verdict switch
        {
            Verdict.Unknown => ThreatZone.Unknown,
            Verdict.Undetected => ThreatZone.Green,
            Verdict.Suspicious => ThreatZone.Yellow,
            Verdict.Malicious => ThreatZone.Red,
            _ => throw new InvalidOperationException("Verdict at FileAnalyzer of Filescan service was out of range.")
        };

        var report = Report.Create(
            filescanReportId,
            verdict,
            threatZone,
            filescanReport.FinalVerdict?.ThreatLevel);
        serviceAnalysis.AddReport(report);

        return serviceAnalysis;
    }
}