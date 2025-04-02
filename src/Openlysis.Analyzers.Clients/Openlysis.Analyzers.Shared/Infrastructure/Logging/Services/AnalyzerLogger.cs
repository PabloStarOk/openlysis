using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Core.Configuration;
using Openlysis.Analyzers.Shared.Infrastructure.Logging.Abstractions;

namespace Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;

/// <summary>
/// Provides logging functionality for analyzers with specified options.
/// </summary>
/// <typeparam name="TOptions">The type of the analyzer options.</typeparam>
public class AnalyzerLogger<TOptions> : IAnalyzerLogger
    where TOptions : AnalyzerOptions
{
    /// <summary>
    /// The logger instance used for logging messages.
    /// </summary>
    protected readonly ILogger<AnalyzerLogger<TOptions>> _logger;

    /// <summary>
    /// The options monitor for accessing the current analyzer options.
    /// </summary>
    protected readonly IOptionsMonitor<TOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzerLogger{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options for the analyzer.</param>
    public AnalyzerLogger(
        ILogger<AnalyzerLogger<TOptions>> logger,
        IOptionsMonitor<TOptions> options)
    {
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task LogNonSuccessStatusCodeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        string requestString = await GetFormattedHttpRequestAsync(response.RequestMessage, cancellationToken);
        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogError(
            "{ServiceName}: Response status code was not successful: "
            + "\nResponse:"
            + "\t\nStatusCode: {StatusCode}"
            + "\t\nResponse Content: {Response}"
            + "\nRequest:"
            + "\t\n{Request}",
            _options.CurrentValue.ServiceName,
            response.StatusCode,
            responseString,
            requestString);
    }

    /// <summary>
    /// Formats the HTTP request message into a readable string.
    /// </summary>
    /// <param name="request">The HTTP request message to format.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A formatted string representing the HTTP request message.</returns>
    protected async Task<string> GetFormattedHttpRequestAsync(
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

        if (request is not null)
        {
            bool containsApiKeyHeader = request.Headers
                .Any(h => h.Key == _options.CurrentValue.ApiKeyHeaderName
                    && !string.IsNullOrWhiteSpace(h.Value.ToString()));
            stringBuilder.AppendLine($"Contains API Key: {containsApiKeyHeader.ToString()}");
        }

        if (request?.Content is not null)
        {
            string requestString = await request.Content.ReadAsStringAsync(cancellationToken);
            stringBuilder.AppendLine($"Request Body: {requestString}");
        }

        return stringBuilder.ToString();
    }
}