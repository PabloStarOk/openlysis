using System.Collections.Concurrent;

using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.TestTools.ServicesSimulation.Common.Configuration;
using Openlysis.TestTools.ServicesSimulation.Common.Constants;
using Openlysis.TestTools.ServicesSimulation.Common.Enums;
using Openlysis.TestTools.ServicesSimulation.Common.Models;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

/// <summary>
/// Simulates an analyzer service that processes analysis requests and maintains their state.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis performed by this analyzer.</typeparam>
/// <typeparam name="TStubFactoryOptions">The options type used by the stub factory to create analysis objects.</typeparam>
internal sealed class AnalysisBehaviorSimulator<TAnalysis, TStubFactoryOptions>
    : IDisposable, IAsyncDisposable
    where TAnalysis : ServiceAnalysis
    where TStubFactoryOptions : AnalysisStubFactoryOptions
{
    private readonly ILogger<AnalysisBehaviorSimulator<TAnalysis, TStubFactoryOptions>> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<ComposedServiceAnalysisId, AnalysisProcess<TAnalysis>> _processes = [];
    private readonly TimeProvider _timeProvider;
    private readonly AnalysisStubBuilder<TStubFactoryOptions, TAnalysis> _analysisBuilder;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisBehaviorSimulator{TAnalysis, TStubFactoryOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostic and tracing information.</param>
    /// <param name="loggerFactory">The factory for creating loggers for child components.</param>
    /// <param name="timeProvider">The time provider for creating time-based operations.</param>
    /// <param name="analysisBuilder">The factory that creates analysis stubs for simulation purposes.</param>
    public AnalysisBehaviorSimulator(
        ILogger<AnalysisBehaviorSimulator<TAnalysis, TStubFactoryOptions>> logger,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        AnalysisStubBuilder<TStubFactoryOptions, TAnalysis> analysisBuilder)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _timeProvider = timeProvider;
        _analysisBuilder = analysisBuilder;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        IEnumerable<Task> disposeProcesses = _processes.Values
            .Select(p => p.DisposeAsync().AsTask());
        await Task.WhenAll(disposeProcesses);
        _processes.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var process in _processes.Values)
        {
            process.Dispose();
        }

        _processes.Clear();
    }

    /// <summary>
    /// Handles an analysis request asynchronously.
    /// </summary>
    /// <param name="analyzerOptions">The options that configure the behavior of this analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing either the created analysis
    /// or an error if the operation fails.
    /// </returns>
    internal async Task<ErrorOr<TAnalysis>> SimulateAnalyzeAsync(
        AnalysisServiceOptions<TStubFactoryOptions> analyzerOptions,
        CancellationToken cancellationToken = default)
    {
        LogRequestReceived(analyzerOptions, nameof(SimulateAnalyzeAsync));

        SimulatedEndpointOptions endpointOptions = analyzerOptions.AnalyzeEndpoint;
        if (endpointOptions.ReturnError)
        {
            return SimulationErrors.Analyze;
        }

        var latency = TimeSpan.FromMilliseconds(endpointOptions.LatencyMs);
        await Task.Delay(latency, cancellationToken);

        var analysis = _analysisBuilder.Create(
            analyzerOptions.Name,
            analyzerOptions.StubFactory);
        CreateProcess(analyzerOptions, analysis);
        return analysis;
    }

    /// <summary>
    /// Retrieves the current status of an analysis asynchronously.
    /// </summary>
    /// <param name="analyzerOptions">The options that configure the behavior of this analysis service.</param>
    /// <param name="id">The unique identifier of the analysis whose status is being requested.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing either the current status
    /// of the requested analysis or an error if the operation fails.
    /// </returns>
    internal async Task<ErrorOr<AnalysisStatus>> SimulateGetStatusAsync(
        AnalysisServiceOptions<TStubFactoryOptions> analyzerOptions,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        LogRequestReceived(analyzerOptions, nameof(SimulateGetStatusAsync));

        SimulatedEndpointOptions endpointOptions = analyzerOptions.GetStatusEndpoint;
        if (endpointOptions.ReturnError)
        {
            return SimulationErrors.GetAnalysisStatus;
        }

        var latency = TimeSpan.FromMilliseconds(endpointOptions.LatencyMs);
        await Task.Delay(latency, cancellationToken);

        return _processes[id].Analysis.State.Status;
    }

    /// <summary>
    /// Retrieves a completed analysis by its identifier asynchronously.
    /// </summary>
    /// <param name="analyzerOptions">The options that configure the behavior of this analysis service.</param>
    /// <param name="id">The unique identifier of the analysis to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing either the requested analysis
    /// or an error if the operation fails.
    /// </returns>
    internal async Task<ErrorOr<TAnalysis>> SimulateGetAnalysisAsync(
        AnalysisServiceOptions<TStubFactoryOptions> analyzerOptions,
        ComposedServiceAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        LogRequestReceived(analyzerOptions, nameof(SimulateGetAnalysisAsync));

        SimulatedEndpointOptions endpointOptions = analyzerOptions.GetAnalysisEndpoint;
        if (endpointOptions.ReturnError)
        {
            return SimulationErrors.GetAnalysis;
        }

        var latency = TimeSpan.FromMilliseconds(endpointOptions.LatencyMs);
        await Task.Delay(latency, cancellationToken);

        return _processes[id].Analysis;
    }

    /// <summary>
    /// Creates a duration value in seconds for the analysis process based on the configured simulation type.
    /// </summary>
    /// <param name="durationOptions">The options containing the analysis duration configuration.</param>
    /// <returns>
    /// The duration in seconds that the analysis process should run for.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the simulation type is not supported or when configuration values are not valid.
    /// </exception>
    private static int CreateAnalysisDuration(AnalysisDurationOptions durationOptions)
    {
        return durationOptions.SimulationType switch
        {
            SimulationType.Fixed => durationOptions.FixedValue,
            SimulationType.Range => GetRandomDurationFromRange(durationOptions.ValuesRange),
            SimulationType.Set => GetRandomDurationFromSet(durationOptions.ValuesSet),
            SimulationType.Random => throw new InvalidOperationException("Random duration for an analysis is not available."),
            _ => throw new InvalidOperationException("Unknown analysis type."),
        };
    }

    /// <summary>
    /// Gets a random duration in seconds from the configured range of values.
    /// </summary>
    /// <param name="range">The range specifying minimum and maximum duration values.</param>
    /// <returns>A randomly selected integer value between the minimum and maximum of the range.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the range is null.</exception>
    private static int GetRandomDurationFromRange(SimulationRange<int>? range)
    {
        if (range is null)
        {
            throw new InvalidOperationException("Range of durations for an analysis is null.");
        }

        return Random.Shared.Next(range.Minimum, range.Maximum);
    }

    /// <summary>
    /// Gets a random duration in seconds from the configured set of values.
    /// </summary>
    /// <param name="set">The set of integer values representing possible durations.</param>
    /// <returns>A randomly selected integer value from the provided set of possible durations.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the set is null or contains fewer than two values.</exception>
    private static int GetRandomDurationFromSet(HashSet<int> set)
    {
        int randomIndex = Random.Shared.Next(0, set.Count);
        return set.ToList()[randomIndex];
    }

    /// <summary>
    /// Creates a new analysis process for the given analysis and starts tracking it.
    /// </summary>
    /// <param name="analyzerOptions">The options that configure the analyzer behavior.</param>
    /// <param name="analysis">The analysis for which to create a process.</param>
    /// <remarks>
    /// The created process is added to the <see cref="_processes"/> dictionary using the analysis ID as the key.
    /// The process is started with the configured analysis duration and will use the stub factory to create
    /// a finished analysis when complete.
    /// </remarks>
    private void CreateProcess(
        AnalysisServiceOptions<TStubFactoryOptions> analyzerOptions,
        TAnalysis analysis)
    {
        var processLogger = _loggerFactory.CreateLogger<AnalysisProcess<TAnalysis>>();
        var analysisProcess = new AnalysisProcess<TAnalysis>(processLogger, analysis, _timeProvider);
        int secondsDuration =
            CreateAnalysisDuration(analyzerOptions.AnalysisSecondsDuration);
        analysisProcess.Start(
            processAnalysis => _analysisBuilder.Finalize(
                processAnalysis,
                analyzerOptions.StubFactory),
            secondsDuration);
        _processes.TryAdd(analysis.Id, analysisProcess);

        LogCreatedProcess(analysisProcess, secondsDuration);
    }

    /// <summary>
    /// Logs a debug message indicating that a request was received and is being processed.
    /// </summary>
    /// <param name="analyzerOptions">The options that configure the analyzer's behavior for this request.</param>
    /// <param name="methodName">The name of the method that is handling the request.</param>
    private void LogRequestReceived(
        AnalysisServiceOptions<TStubFactoryOptions> analyzerOptions,
        string methodName)
    {
        _logger.LogDebug(
            "{ServiceName}: Behavior simulated with {MethodName} method.",
            analyzerOptions.Name,
            methodName);
    }

    /// <summary>
    /// Logs debug information about a newly created analysis process.
    /// </summary>
    /// <param name="process">The analysis process that was created.</param>
    /// <param name="secondsDuration">The duration in seconds that the process will run for.</param>
    private void LogCreatedProcess(
        AnalysisProcess<TAnalysis> process,
        int secondsDuration)
    {
        _logger.LogTrace(
            "Analysis process created:"
            + "\n\tAnalysis Type: {AnalysisType}"
            + "\n\tID: {Id}"
            + "\n\tDuration: {Duration} seconds",
            process.Analysis.GetType(),
            process.Analysis.Id,
            secondsDuration);
    }
}