using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.TestTools.ServicesSimulation.Common.Services.Analyzers;

/// <summary>
/// Represents a process that manages the lifecycle of an analysis operation.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis being processed.</typeparam>
internal sealed class AnalysisProcess<TAnalysis>
    : IDisposable, IAsyncDisposable
    where TAnalysis : Analysis
{
    /// <summary>
    /// Gets the analysis object being processed.
    /// </summary>
    internal TAnalysis Analysis { get; }

    /// <summary>
    /// Occurs when the analysis process has been finalized.
    /// </summary>
    internal event EventHandler<ExternalAnalysisId>? Finalized;

    private readonly ILogger<AnalysisProcess<TAnalysis>> _logger;
    private readonly Stopwatch _stopwatch = new ();
    private readonly ITimer _timer;
    private CancellationTokenRegistration _cancellationTokenRegistration;
    private Action<TAnalysis>? _finalizeAnalysis;
    private bool _disposed;
    private bool _started;
    private bool _finished;

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
        _cancellationTokenRegistration.Dispose();
        _finalizeAnalysis = null;
        Finalized = null;
        LogDispose();
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
        await _cancellationTokenRegistration.DisposeAsync();
        _finalizeAnalysis = null;
        Finalized = null;
        LogDispose();
    }

    /// <summary>
    /// Starts the analysis process with the specified finalization action and duration.
    /// </summary>
    /// <param name="finalizeAnalysis">The action to execute when the process completes.</param>
    /// <param name="secondsDuration">The duration of the analysis in seconds.</param>
    /// <exception cref="InvalidOperationException">Thrown when the analysis process has already been started.</exception>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    internal void Start(
        Action<TAnalysis> finalizeAnalysis,
        int secondsDuration,
        CancellationToken cancellationToken)
    {
        if (_started)
        {
            throw new InvalidOperationException("Analysis process already started.");
        }

        _started = true;
        _cancellationTokenRegistration = cancellationToken.Register(Cancel);
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
        if (_cancellationTokenRegistration.Token.IsCancellationRequested)
        {
            return;
        }

        _finished = true;
        _timer.Dispose();

        if (_finalizeAnalysis is null)
        {
            throw new InvalidOperationException("Analysis process has not been started.");
        }

        _finalizeAnalysis.Invoke(Analysis);
        Finalized?.Invoke(this, Analysis.ExternalId);
        StopAndLogDuration(action: "stopped");
    }

    /// <summary>
    /// Cancels the analysis process, stops the timer and logs the cancellation event.
    /// </summary>
    private void Cancel()
    {
        if (_finished)
        {
            return;
        }

        _cancellationTokenRegistration.Unregister();
        _timer.Dispose();
        Finalized?.Invoke(this, Analysis.ExternalId);
        StopAndLogDuration(action: "canceled");
    }

    /// <summary>
    /// Logs a debug message indicating that the analysis process has been disposed.
    /// </summary>
    private void LogDispose()
    {
        _logger.LogDebug(
            "Disposing analysis process for {AnalysisType}.",
            typeof(TAnalysis));
    }

    /// <summary>
    /// Stops the stopwatch and logs the duration of the analysis process with the specified action.
    /// </summary>
    /// <param name="action">A string describing the action that ended the process (e.g., "stopped", "canceled").</param>
    private void StopAndLogDuration(string action)
    {
        _stopwatch.Stop();
        _logger.LogDebug(
            "Analysis process for {AnalysisType} was {Action} after {Duration} seconds.",
            typeof(TAnalysis),
            action,
            _stopwatch.Elapsed.TotalSeconds);
    }
}