using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Logging;

using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Configuration;

namespace Openlysis.MultiAnalyzer.Models;

/// <summary>
/// Represents a timeout request for handling operations of type <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request to process.</typeparam>
internal sealed class TimeoutRequest<TRequest> : IDisposable, IAsyncDisposable
    where TRequest : class
{
    /// <summary>
    /// Gets the unique identifier for this timeout request instance.
    /// </summary>
    internal Guid Id { get; }

    private readonly ILogger<TimeoutRequest<TRequest>> _logger;
    private readonly TimeoutRequestOptions _options;
    private readonly ITimer _timeoutTimer;
    private readonly IRequestOrchestrator<TRequest> _orchestrator;
    private CancellationTokenSource _timeoutTokenSource;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutRequest{TRequest}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging timeout request events.</param>
    /// <param name="id">The unique identifier for this timeout request instance.</param>
    /// <param name="options">The options for configuring the timeout request.</param>
    /// <param name="timeProvider">The time provider used to create timers for timeouts.</param>
    /// <param name="orchestrator">The orchestrator logic handler for processing the timeout request.</param>
    internal TimeoutRequest(
        ILogger<TimeoutRequest<TRequest>> logger,
        Guid id,
        TimeoutRequestOptions options,
        TimeProvider timeProvider,
        IRequestOrchestrator<TRequest> orchestrator)
    {
        Id = id;
        _logger = logger;
        _options = options;
        _orchestrator = orchestrator;

        _timeoutTimer = timeProvider.CreateTimer(
            _ => _timeoutTokenSource?.Cancel(),
            null,
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _disposed = true;
        _timeoutTimer.Dispose();
        _timeoutTokenSource.Dispose();

#if DEBUG
        _logger.LogDebug("Disposing timeout request.");
#endif
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _disposed = true;
        await _timeoutTimer.DisposeAsync().ConfigureAwait(false);
        _timeoutTokenSource.Dispose();

#if DEBUG
        _logger.LogDebug("Disposing timeout request.");
#endif
    }

    /// <summary>
    /// Processes the specified request asynchronously, applying a timeout as configured.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task ProcessAsync(
        ConsumeContext<TRequest> request,
        CancellationToken cancellationToken)
    {
        _timeoutTokenSource = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);

        _timeoutTimer.Change(
            TimeSpan.FromSeconds(_options.SecondsTimeout),
            Timeout.InfiniteTimeSpan);

        try
        {
#if DEBUG
            _stopwatch.Start();
#endif
            await _orchestrator
                .ExecuteAsync(request, _timeoutTokenSource.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        when (ex.CancellationToken == _timeoutTokenSource.Token)
        {
            await _orchestrator.OnTimeoutAsync();
#if DEBUG
            LogTimeout();
#endif
        }

        _timeoutTimer.Change(
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan);
    }

#if DEBUG
    private readonly Stopwatch _stopwatch = new ();

    /// <summary>
    /// Logs a debug message indicating that the timeout request has timed out,
    /// including the request type, elapsed time in seconds, and timeout request ID.
    /// </summary>
    private void LogTimeout()
    {
        _stopwatch.Stop();

        _logger.LogDebug(
            "{RequestType} request timed out after {Seconds} seconds with timeout request ID: {TimeoutRequestId}",
            typeof(TRequest),
            _stopwatch.Elapsed.TotalSeconds,
            Id);
    }
#endif
}
