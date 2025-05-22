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
    : Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>
{
    private readonly string _optionsName;
    private readonly AnalysisBehaviorSimulator<UrlServiceAnalysis, AnalysisStubFactoryOptions> _behaviorSimulator;
    private readonly IDisposable? _optionsObserver;
    private AnalysisServiceOptions<AnalysisStubFactoryOptions> _analyzerOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedUrlAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The analyzer configuration options.</param>
    /// <param name="rateQuotaService">The service that manages rate quotas for analysis endpoints.</param>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="logger">The logger for this analyzer.</param>
    /// <param name="optionsName">The name of the options to retrieve from the monitor.</param>
    /// <param name="analyzerOptions">The monitor for analyzer-specific options.</param>
    /// <param name="behaviorSimulator">The simulator that provides simulated analysis behavior.</param>
    public SimulatedUrlAnalyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        IServiceLogger<SimulatedUrlAnalyzer> logger,
        string optionsName,
        IOptionsMonitor<AnalysisServiceOptions<AnalysisStubFactoryOptions>> analyzerOptions,
        AnalysisBehaviorSimulator<UrlServiceAnalysis, AnalysisStubFactoryOptions> behaviorSimulator)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _optionsName = optionsName;
        _analyzerOptions = analyzerOptions.Get(optionsName);
        _behaviorSimulator = behaviorSimulator;
        _optionsObserver = analyzerOptions.OnChange(OnOptionsChanged);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _behaviorSimulator.SimulateAnalyzeAsync(
            _analyzerOptions,
            cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        return await _behaviorSimulator.SimulateGetStatusAsync(
            _analyzerOptions,
            id,
            cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<UrlServiceAnalysis>> OnGetAnalysisAsync(
        HttpClient httpClient,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        return await _behaviorSimulator.SimulateGetAnalysisAsync(
            _analyzerOptions,
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
    /// Handles changes to the analyzer options configuration.
    /// </summary>
    /// <param name="changedOptions">The updated analyzer options configuration.</param>
    /// <param name="name">The name of the options that changed. Used to identify if the change is relevant to this instance.</param>
    private void OnOptionsChanged(
        AnalysisServiceOptions<AnalysisStubFactoryOptions> changedOptions,
        string? name)
    {
        if (name is null || !name.Equals(_optionsName))
        {
            return;
        }

        _analyzerOptions = changedOptions;
        _logger.LogDebug("Options with name {Name} were changed", name);
    }
}