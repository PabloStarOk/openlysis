using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Evaluators.Shared.Configuration;
using Openlysis.Infrastructure.Shared.Logging.Abstractions;

namespace Openlysis.Evaluators.Shared.Infrastructure.Logging.Services;

/// <summary>
/// Provides logging functionality for analyzers with specified options.
/// </summary>
/// <typeparam name="TOptions">The type of the options used by the analyzer.</typeparam>
public class ReputationEvaluatorLogger<TOptions> : ServiceLogger
    where TOptions : ReputationEvaluatorOptions
{
    /// <summary>
    /// The options monitor for accessing the current analyzer options.
    /// </summary>
    private readonly IOptionsMonitor<TOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReputationEvaluatorLogger{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="options">The options for the analyzer.</param>
    public ReputationEvaluatorLogger(
        ILogger<ReputationEvaluatorLogger<TOptions>> logger,
        IOptionsMonitor<TOptions> options)
        : base(logger)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public override async Task LogNonSuccessStatusCodeAsync(
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