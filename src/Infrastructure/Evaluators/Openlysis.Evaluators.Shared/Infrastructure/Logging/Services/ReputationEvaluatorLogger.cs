using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;
using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Evaluators.Shared.Infrastructure.Logging.Services;

/// <summary>
/// Provides logging functionality for analyzers with specified options.
/// </summary>
/// <typeparam name="TCategoryName">The category name type used for logging.</typeparam>
/// <remarks>
/// This logger is designed to work with analyzers that require specific configuration options
/// and provides methods for logging HTTP responses and other events.
/// </remarks>
public class ReputationEvaluatorLogger<TCategoryName>
    : IServiceLogger<TCategoryName>
    where TCategoryName : notnull
{
    /// <summary>
    /// Gets the underlying logger instance used for logging operations.
    /// </summary>
    private ILogger<TCategoryName> Logger { get; }

    /// <summary>
    /// Gets the options monitor for accessing the current evaluator options.
    /// </summary>
    private IOptionsMonitor<ServiceOptions> Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReputationEvaluatorLogger{TCategoryName}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options for the evaluator.</param>
    public ReputationEvaluatorLogger(
        ILogger<TCategoryName> logger,
        IOptionsMonitor<ServiceOptions> options)
    {
        Logger = logger;
        Options = options;
    }

    /// <inheritdoc/>
    public async Task LogNonSuccessStatusCodeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        string requestString = await GetFormattedHttpRequestAsync(response.RequestMessage, cancellationToken);
        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        Logger.LogError(
            "{ServiceName}: Response status code was not successful: "
            + "\nResponse:"
            + "\t\nStatusCode: {StatusCode}"
            + "\t\nResponse Content: {Response}"
            + "\nRequest:"
            + "\t\n{Request}",
            Options.CurrentValue.ServiceName,
            response.StatusCode,
            responseString,
            requestString);
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Logger.Log(logLevel, eventId, state, exception, formatter);
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return Logger.IsEnabled(logLevel);
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return Logger.BeginScope(state);
    }

    /// <summary>
    /// Formats the HTTP request message into a readable string.
    /// </summary>
    /// <param name="request">The HTTP request message to format.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A formatted string representing the HTTP request message.</returns>
    private static async Task<string> GetFormattedHttpRequestAsync(
        HttpRequestMessage? request,
        CancellationToken cancellationToken = default)
    {
        var stringBuilder = new StringBuilder();

        if (request?.Method is not null)
        {
            stringBuilder.AppendLine($"Method: {request.Method}");
        }

        if (request?.RequestUri is not null)
        {
            stringBuilder.AppendLine($"Request URI: {request.RequestUri.AbsoluteUri}");
        }

        if (request?.Content is not null)
        {
            string requestString = await request.Content.ReadAsStringAsync(cancellationToken);
            stringBuilder.AppendLine($"Request Body: {requestString}");
        }

        return stringBuilder.ToString();
    }
}