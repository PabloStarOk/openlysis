using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

namespace Openlysis.TestTools.ServicesSimulation.URLs.Infrastructure;

/// <summary>
/// Simulates a URL analyzer service for testing purposes.
/// This class provides mock implementations of URL analysis operations
/// without making actual service calls.
/// </summary>
internal sealed class SimulatedUrlAnalyzer
    : Analyzer<UrlAnalysis, AnalyzeUrlRequest>
{
    private readonly string _optionsName;
    private readonly AnalysisBehaviorSimulator<UrlAnalysis, AnalysisStubFactoryOptions> _behaviorSimulator;
    private readonly IDisposable? _optionsObserver;
    private AnalysisServiceOptions<AnalysisStubFactoryOptions> _analyzerServiceOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedUrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The analyzer configuration serviceOptions.</param>
    /// <param name="rateQuotaService">The service that manages rate quotas for analysis endpoints.</param>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="logger">The logger for this analyzer.</param>
    /// <param name="optionsName">The name of the serviceOptions to retrieve from the monitor.</param>
    /// <param name="analyzerOptions">The monitor for analyzer-specific serviceOptions.</param>
    /// <param name="behaviorSimulator">The simulator that provides simulated analysis behavior.</param>
    public SimulatedUrlAnalyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        IServiceLogger<SimulatedUrlAnalyzer> logger,
        string optionsName,
        IOptionsMonitor<AnalysisServiceOptions<AnalysisStubFactoryOptions>> analyzerOptions,
        AnalysisBehaviorSimulator<UrlAnalysis, AnalysisStubFactoryOptions> behaviorSimulator)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _optionsName = optionsName;
        _analyzerServiceOptions = analyzerOptions.Get(optionsName);
        _behaviorSimulator = behaviorSimulator;
        _optionsObserver = analyzerOptions.OnChange(OnOptionsChanged);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _behaviorSimulator.SimulateAnalyzeAsync(
            _analyzerServiceOptions,
            cancellationToken);

        if (result.IsError)
        {
            return result;
        }

        // Return a copy of the same object but different memory address direction, to avoid interfering with real libraries' functionality.
        return UrlAnalysis.CreateWithId(
            result.Value.Id,
            result.Value.ExternalId,
            result.Value.ServiceName,
            result.Value.State.Status,
            result.Value.State.Verdict,
            result.Value.ThreatScore);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ExternalAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        return await _behaviorSimulator.SimulateGetStatusAsync(
            _analyzerServiceOptions,
            id,
            cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ExternalAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        return await _behaviorSimulator.SimulateGetAnalysisAsync(
            _analyzerServiceOptions,
            id,
            cancellationToken);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        _optionsObserver?.Dispose();
    }

    /// <summary>
    /// Handles changes to the analyzer serviceOptions configuration.
    /// </summary>
    /// <param name="changedServiceOptions">The updated analyzer serviceOptions configuration.</param>
    /// <param name="name">The name of the serviceOptions that changed. Used to identify if the change is relevant to this instance.</param>
    private void OnOptionsChanged(
        AnalysisServiceOptions<AnalysisStubFactoryOptions> changedServiceOptions,
        string? name)
    {
        if (name is null || !name.Equals(_optionsName))
        {
            return;
        }

        _analyzerServiceOptions = changedServiceOptions;
        _logger.LogDebug("Options with name {Name} were changed", name);
    }
}