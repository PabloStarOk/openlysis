using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Configuration;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Responses;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Analyzers;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Factories;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;

using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Constants;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Analyzers.VirusTotal.Adapters;

/// <summary>
/// Analyzes files using VirusTotal's API to determine potential security threats.
/// </summary>
/// <remarks>
/// Supports both standard and large file uploads, handling the different submission requirements
/// and processing the returned analysis results from VirusTotal's service.
/// </remarks>
internal class FileAnalyzer : Analyzer<FileAnalysis, AnalyzeFileRequest>
{
    private readonly IOptionsMonitor<VirusTotalAnalyzerOptions> _analyzerOptions;
    private readonly ILargeFileUploadProvider _largeFileUploadProvider;
    private readonly IVirusTotalAnalyzer _vtAnalyzer;
    private readonly IVerdictCalculator _verdictCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalyzer"/> class.
    /// </summary>
    /// <param name="options">Configuration options for the analyzer.</param>
    /// <param name="rateQuotaService">Service for tracking and limiting API call rates.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <param name="largeFileUploadProvider">Provider for handling large file uploads.</param>
    /// <param name="vtAnalyzer">The VirusTotal analyzer implementation.</param>
    /// <param name="verdictCalculator">Calculator for determining analysis verdicts.</param>
    public FileAnalyzer(
        IOptionsMonitor<VirusTotalAnalyzerOptions> options,
        [FromKeyedServices(UrlAnalyzer.LimitTrackerServiceKey)] IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(VirusTotalAnalyzer.KeyedServicesKey)] IServiceLogger<FileAnalyzer> logger,
        ILargeFileUploadProvider largeFileUploadProvider,
        IVirusTotalAnalyzer vtAnalyzer,
        IVerdictCalculator verdictCalculator)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _analyzerOptions = options;
        _largeFileUploadProvider = largeFileUploadProvider;
        _vtAnalyzer = vtAnalyzer;
        _verdictCalculator = verdictCalculator;
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

        string? fileUploadUrl = null;
        if (request.FileData.Length > Files.SmallFilesMaxSizeInBytes)
        {
            ErrorOr<string> getUploadUrlResult =
                await _largeFileUploadProvider.GetFileUploadUrlAsync(cancellationToken);
            if (getUploadUrlResult.IsError)
            {
                return getUploadUrlResult.Errors;
            }

            fileUploadUrl = getUploadUrlResult.Value;
        }

        var factory = new FileRequestFactory(request, fileUploadUrl);
        ErrorOr<AnalyzeResponse> result = await _vtAnalyzer.AnalyzeAsync(
            httpClient,
            factory,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        AnalyzeResponse analyzeResponse = result.Value;

        return FileAnalysis.Create(
            analyzeResponse.AnalysisId,
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
        ErrorOr<GetAnalysisResponse> result = await _vtAnalyzer.GetAnalysisAsync(
            httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value.Attributes.Status];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ErrorOr<GetAnalysisResponse> result = await _vtAnalyzer.GetAnalysisAsync(
            httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        GetAnalysisResponse analysis = result.Value;

#if DEBUG
        DebugFileAnalysis(analysis);
#endif

        return CreateAnalysisFromResponse(analysis);
    }

    /// <summary>
    /// Creates a FileServiceAnalysis object from the VirusTotal analysis response.
    /// </summary>
    /// <param name="analysisResponse">The response data from VirusTotal's analysis.</param>
    /// <returns>A configured FileServiceAnalysis object with updated verdict and status.</returns>
    private FileAnalysis CreateAnalysisFromResponse(
        GetAnalysisResponse analysisResponse)
    {
        AnalysisStatus status = Maps.AnalysisStatusMap[
            analysisResponse.Attributes.Status];
        Verdict verdict = _verdictCalculator.Calculate(
            analysisResponse.Attributes.Stats);

        var serviceAnalysis = FileAnalysis.Create(
            analysisResponse.Id,
            ServiceName,
            status,
            verdict);

        return serviceAnalysis;
    }

#if DEBUG
    /// <summary>
    /// Logs debug information about the file analysis response from VirusTotal.
    /// </summary>
    /// <param name="analysisResponse">The response containing file analysis results from VirusTotal.</param>
    private void DebugFileAnalysis(
        GetAnalysisResponse analysisResponse)
    {
        _logger.LogDebug(
            "VirusTotal file analysis results:"
            + "\n\tHarmless: {Harmless}"
            + "\n\tUndetected: {Undetected}"
            + "\n\tSuspicious: {Suspicious}"
            + "\n\tMalicious: {Malicious}"
            + "\n\tTimeout: {Timeout}",
            analysisResponse.Attributes.Stats.Harmless,
            analysisResponse.Attributes.Stats.Undetected,
            analysisResponse.Attributes.Stats.Suspicious,
            analysisResponse.Attributes.Stats.Malicious,
            analysisResponse.Attributes.Stats.Timeout);
    }
#endif
}