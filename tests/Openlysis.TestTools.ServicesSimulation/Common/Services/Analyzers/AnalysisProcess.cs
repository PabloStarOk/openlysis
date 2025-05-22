using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

/// <summary>
/// Represents a process that manages the lifecycle of an analysis operation.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis being processed.</typeparam>
internal sealed class AnalysisProcess<TAnalysis>
    : IDisposable, IAsyncDisposable
    where TAnalysis : ServiceAnalysis
{
    /// <summary>
    /// Gets the analysis object being processed.
    /// </summary>
    internal TAnalysis Analysis { get; }

    private readonly ILogger<AnalysisProcess<TAnalysis>> _logger;
    private readonly Stopwatch _stopwatch = new ();
    private readonly ITimer _timer;
    private Action<TAnalysis>? _finalizeAnalysis;
    private bool _disposed;
    private bool _started;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisProcess{TAnalysis}"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging process events.</param>
    /// <param name="analysis">The analysis object to be processed.</param>
    /// <param name="timeProvider">The time provider used for timing operations.</param>
    internal AnalysisProcess(
        ILogger<AnalysisProcess<TAnalysis>> logger,
        TAnalysis analysis,
        TimeProvider timeProvider)
    {
        _logger = logger;
        Analysis = analysis;
        _timer = timeProvider.CreateTimer(
            _ => Stop(),
            null,
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _timer.Dispose();
        _finalizeAnalysis = null;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await _timer.DisposeAsync();
        _finalizeAnalysis = null;
    }

    /// <summary>
    /// Starts the analysis process with the specified finalization action and duration.
    /// </summary>
    /// <param name="finalizeAnalysis">The action to execute when the process completes.</param>
    /// <param name="secondsDuration">The duration of the analysis in seconds.</param>
    /// <exception cref="InvalidOperationException">Thrown when the analysis process has already been started.</exception>
    internal void Start(
        Action<TAnalysis> finalizeAnalysis,
        int secondsDuration)
    {
        if (_started)
        {
            throw new InvalidOperationException("Analysis process already started.");
        }

        _started = true;
        Analysis.UpdateStatus(AnalysisStatus.InProgress);
        _finalizeAnalysis = finalizeAnalysis;
        _timer.Change(
            TimeSpan.FromSeconds(secondsDuration),
            Timeout.InfiniteTimeSpan);

        _stopwatch.Start();
    }

    /// <summary>
    /// Stops the analysis process and updates the analysis object with the finished result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the analysis process has not been started.</exception>
    private void Stop()
    {
        _timer.Dispose();

        if (_finalizeAnalysis is null)
        {
            throw new InvalidOperationException("Analysis process has not been started.");
        }

        _finalizeAnalysis.Invoke(Analysis);

        _stopwatch.Stop();
        _logger.LogDebug(
            "Analysis process stopped after {Duration} seconds.",
            _stopwatch.Elapsed.TotalSeconds);
    }
}