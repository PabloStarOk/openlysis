using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;
using Openlysis.Infrastructure.Shared.Contracts.Common.Abstractions;

namespace Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;

/// <summary>
/// Provides logging functionality for analyzers with specified options.
/// </summary>
/// <typeparam name="TCategoryName">The category name for the logger.</typeparam>
/// <typeparam name="TOptions">The type of the analyzer options.</typeparam>
public class AnalyzerLogger<TCategoryName, TOptions>
    : IServiceLogger<TCategoryName>
    where TCategoryName : notnull
    where TOptions : AnalyzerOptions
{
    /// <summary>
    /// Gets the underlying logger instance used for logging operations.
    /// </summary>
    protected ILogger<TCategoryName> Logger { get; }

    /// <summary>
    /// Gets the options monitor for accessing the current analyzer options.
    /// </summary>
    protected IOptionsMonitor<TOptions> Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzerLogger{TCategoryName, TOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options for the analyzer.</param>
    public AnalyzerLogger(
        ILogger<TCategoryName> logger,
        IOptionsMonitor<TOptions> options)
    {
        Logger = logger;
        Options = options;
    }

    /// <inheritdoc/>
    public async Task LogNonSuccessStatusCodeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        string requestString = GetFormattedHttpRequest(response.RequestMessage);
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
    /// <returns>A formatted string representing the HTTP request message.</returns>
    protected string GetFormattedHttpRequest(HttpRequestMessage? request)
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

        if (request is not null)
        {
            bool containsApiKeyHeader = request.Headers
                .Any(h => h.Key == Options.CurrentValue.ApiKeyHeaderName
                    && !string.IsNullOrWhiteSpace(h.Value.ToString()));
            stringBuilder.AppendLine($"Contains API Key: {containsApiKeyHeader.ToString()}");
        }

        return stringBuilder.ToString();
    }
}