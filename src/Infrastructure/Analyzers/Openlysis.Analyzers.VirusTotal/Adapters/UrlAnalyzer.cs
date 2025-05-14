using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Configuration;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Responses;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Analysis;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Factories;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;

namespace Openlysis.Analyzers.VirusTotal.Adapters;

/// <summary>
/// URL analyzer for VirusTotal service.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    /// <summary>
    /// Key of the service for tracking request limits.
    /// </summary>
    public const string LimitTrackerServiceKey = "VirusTotalLimitTracker";

    private readonly IVirusTotalAnalyzer _vtAnalyzer;
    private readonly IVerdictCalculator _verdictCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for VirusTotal analyzer configuration.</param>
    /// <param name="rateQuotaService">The request limit tracker.</param>
    /// <param name="httpClientFactory">The HTTP client factory used for creating HTTP clients.</param>
    /// <param name="logger">The logger instance for logging.</param>
    /// <param name="vtAnalyzer">The VirusTotal analyzer instance.</param>
    /// <param name="verdictCalculator">The verdict calculator instance.</param>
    public UrlAnalyzer(
        IOptionsMonitor<VirusTotalAnalyzerOptions> options,
        [FromKeyedServices(LimitTrackerServiceKey)] IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(VirusTotalAnalyzer.KeyedServicesKey)] IServiceLogger<UrlAnalyzer> logger,
        IVirusTotalAnalyzer vtAnalyzer,
        IVerdictCalculator verdictCalculator)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _vtAnalyzer = vtAnalyzer;
        _verdictCalculator = verdictCalculator;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestFactory = new UrlRequestFactory(request);
        ErrorOr<AnalyzeResponse> result = await _vtAnalyzer.AnalyzeAsync(
            httpClient,
            requestFactory,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        AnalyzeResponse analyzeResponse = result.Value;

        return UrlServiceAnalysis.Create(
            analyzeResponse.AnalysisId,
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
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
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
        _logger.LogDebug(
            "VirusTotal Results:"
            + "\n\tHarmless: {Harmless}"
            + "\n\tUndetected: {Undetected}"
            + "\n\tSuspicious: {Suspicious}"
            + "\n\tMalicious: {Malicious}"
            + "\n\tTimeout: {Timeout}",
            analysis.Attributes.Stats.Harmless,
            analysis.Attributes.Stats.Undetected,
            analysis.Attributes.Stats.Suspicious,
            analysis.Attributes.Stats.Malicious,
            analysis.Attributes.Stats.Timeout);
#endif

        AnalysisStatus status = Maps.AnalysisStatusMap[analysis.Attributes.Status];
        Verdict verdict = _verdictCalculator.Calculate(analysis.Attributes.Stats);
        return UrlServiceAnalysis.Create(
            analysis.Id,
            ServiceName,
            status,
            verdict);
    }
}