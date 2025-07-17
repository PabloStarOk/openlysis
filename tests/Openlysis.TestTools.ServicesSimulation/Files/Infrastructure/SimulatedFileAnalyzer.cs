using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Analyzers.Shared.Infrastructure.RateQuota.Enums;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Infrastructure.RateQuota.Abstractions;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;
using Openlysis.TestTools.ServicesSimulation.Files.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.Files.Infrastructure;

/// <summary>
/// Simulates a file analyzer service for testing purposes.
/// This class provides mock implementations of file analysis operations
/// without making actual service calls.
/// </summary>
internal sealed class SimulatedFileAnalyzer
    : Analyzer<FileAnalysis, AnalyzeFileRequest>
{
    private readonly string _optionsName;
    private readonly AnalysisBehaviorSimulator<FileAnalysis, FileAnalysisStubFactoryOptions> _behaviorSimulator;
    private readonly IDisposable? _optionsObserver;
    private AnalysisServiceOptions<FileAnalysisStubFactoryOptions> _analyzerServiceOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedFileAnalyzer"/> class.
    /// </summary>
    /// <param name="options">The analyzer configuration serviceOptions.</param>
    /// <param name="rateQuotaService">The service for handling rate quota limits.</param>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="logger">The logger for the service.</param>
    /// <param name="optionsName">The name of the serviceOptions to retrieve from the serviceOptions monitor.</param>
    /// <param name="analyzerOptions">The monitor for file analysis configuration serviceOptions.</param>
    /// <param name="behaviorSimulator">The simulator for controlling analysis behavior.</param>
    public SimulatedFileAnalyzer(
        IOptionsMonitor<AnalyzerOptions> options,
        IRateQuotaService<AnalysisEndpointType> rateQuotaService,
        IHttpClientFactory httpClientFactory,
        IServiceLogger<SimulatedFileAnalyzer> logger,
        string optionsName,
        IOptionsMonitor<AnalysisServiceOptions<FileAnalysisStubFactoryOptions>> analyzerOptions,
        AnalysisBehaviorSimulator<FileAnalysis, FileAnalysisStubFactoryOptions> behaviorSimulator)
        : base(options, rateQuotaService, httpClientFactory, logger)
    {
        _optionsName = optionsName;
        _analyzerServiceOptions = analyzerOptions.Get(optionsName);
        _behaviorSimulator = behaviorSimulator;
        _optionsObserver = analyzerOptions.OnChange(OnOptionsChanged);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileAnalysis>> OnAnalyzeAsync(
        HttpClient httpClient,
        AnalyzeFileRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Request received to analyze a file.");

        var result = await _behaviorSimulator.SimulateAnalyzeAsync(
            _analyzerServiceOptions,
            cancellationToken);

        if (result.IsError)
        {
            return result;
        }

        // Return a copy of the same object but different memory address direction, to avoid interfering with real libraries' functionality.
        return FileAnalysis.CreateWithId(
            result.Value.Id,
            result.Value.ExternalId,
            result.Value.ServiceName,
            result.Value.State.Status,
            result.Value.State.Verdict,
            result.Value.Reports.ToList(),
            result.Value.ThreatScore);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<AnalysisStatus>> OnGetStatusAsync(
        HttpClient httpClient,
        ExternalAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Request received to get status a file analysis.");
        return await _behaviorSimulator.SimulateGetStatusAsync(
            _analyzerServiceOptions,
            id,
            cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task<ErrorOr<FileAnalysis>> OnGetAnalysisAsync(
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
        AnalysisServiceOptions<FileAnalysisStubFactoryOptions> changedServiceOptions,
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