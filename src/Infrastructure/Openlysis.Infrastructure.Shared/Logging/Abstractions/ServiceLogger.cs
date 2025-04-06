using Microsoft.Extensions.Logging;

namespace Openlysis.Infrastructure.Shared.Logging.Abstractions;

/// <summary>
/// Defines a basic and custom logger for analyzers.
/// </summary>
public abstract class ServiceLogger : ILogger<ServiceLogger>
{
    /// <summary>
    /// The underlying logger instance used for logging operations.
    /// </summary>
    protected readonly ILogger<ServiceLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceLogger"/> class.
    /// </summary>
    /// <param name="logger">The underlying logger instance used for logging operations.</param>
    protected ServiceLogger(ILogger<ServiceLogger> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs non-success HTTP responses including status code and response content.
    /// </summary>
    /// <param name="response">The HTTP response message containing status and content.</param>
    /// <param name="cancellationToken">Token to propagate notification that operations should be canceled.</param>
    /// <returns>A task that represents the asynchronous logging operation.</returns>
    public abstract Task LogNonSuccessStatusCodeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return _logger.BeginScope(state);
    }
}