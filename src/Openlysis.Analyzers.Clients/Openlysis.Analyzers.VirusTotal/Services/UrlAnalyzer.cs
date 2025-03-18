using ErrorOr;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Contracts.Core.Common.Abstractions;
using Openlysis.Analyzers.Contracts.Core.URLs.Requests;
using Openlysis.Analyzers.Contracts.Infrastructure.RateLimit.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Configuration;
using Openlysis.Analyzers.VirusTotal.Core.Constants;
using Openlysis.Analyzers.VirusTotal.Core.Models.Responses;
using Openlysis.Analyzers.VirusTotal.Infrastructure.Factories;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.Analyzers.VirusTotal.Services;

/// <summary>
/// URL analyzer for VirusTotal service.
/// </summary>
public class UrlAnalyzer : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    /// <summary>
    /// The key used to identify the VirusTotal HTTP client service.
    /// </summary>
    public const string HttpClientServiceKey = "VirusTotal";

    private readonly IVirusTotalAnalyzer _vtAnalyzer;
    private readonly IVerdictCalculator _verdictCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for VirusTotal analyzer configuration.</param>
    /// <param name="requestLimitTracker">The request limit tracker.</param>
    /// <param name="httpClient">The HTTP client used for making requests.</param>
    /// <param name="logger">The logger instance for logging.</param>
    /// <param name="vtAnalyzer">The VirusTotal analyzer instance.</param>
    /// <param name="verdictCalculator">The verdict calculator instance.</param>
    public UrlAnalyzer(
        IOptionsMonitor<VirusTotalAnalyzerOptions> options,
        IRequestLimitTracker requestLimitTracker,
        [FromKeyedServices(HttpClientServiceKey)] HttpClient httpClient,
        ILogger<UrlAnalyzer> logger,
        IVirusTotalAnalyzer vtAnalyzer,
        IVerdictCalculator verdictCalculator)
        : base(options, requestLimitTracker, httpClient, logger)
    {
        _vtAnalyzer = vtAnalyzer;
        _verdictCalculator = verdictCalculator;
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(AnalyzeUrlRequest request, CancellationToken cancellationToken = default)
    {
        var requestFactory = new UrlRequestFactory(request);
        ErrorOr<AnalyzeUrlResponse> result = await _vtAnalyzer.AnalyzeAsync(
            _httpClient,
            requestFactory,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        AnalyzeUrlResponse analyzeUrlResponse = result.Value;

        return UrlServiceAnalysis.Create(
            analyzeUrlResponse.AnalysisId,
            ServiceName,
            AnalysisStatus.Queued,
            Verdict.Unknown);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(ComposedServiceAnalysisId id, CancellationToken cancellationToken = default)
    {
        ErrorOr<GetAnalysisResponse> result = await _vtAnalyzer.GetAnalysisAsync(
            _httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Maps.AnalysisStatusMap[result.Value.Attributes.Status];
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnGetAnalysisAsync(ComposedServiceAnalysisId id, CancellationToken cancellationToken = default)
    {
        ErrorOr<GetAnalysisResponse> result = await _vtAnalyzer.GetAnalysisAsync(
            _httpClient,
            id.Primary.Value,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        GetAnalysisResponse analysis = result.Value;
        AnalysisStatus status = Maps.AnalysisStatusMap[analysis.Attributes.Status];
        Verdict verdict = _verdictCalculator.Calculate(analysis.Attributes.Stats);
        return UrlServiceAnalysis.Create(
            analysis.Id,
            ServiceName,
            status,
            verdict);
    }
}