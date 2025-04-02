using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Configuration;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Services;

/// <summary>
/// Represents a logger for the sandbox analyzer of Hybrid Analysis service.
/// </summary>
/// <remarks>
/// Inherits from <see cref="AnalyzerLogger{TOptions}"/>.
/// </remarks>
public class SandboxAnalyzerLogger : AnalyzerLogger<HybridAnalyzerOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxAnalyzerLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The options monitor for <see cref="HybridAnalyzerOptions"/>.</param>
    public SandboxAnalyzerLogger(
        ILogger<SandboxAnalyzerLogger> logger,
        IOptionsMonitor<HybridAnalyzerOptions> options)
        : base(logger, options)
    {
    }

    /// <summary>
    /// Logs an error state asynchronously.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="reportState">The report state response containing error details.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LogStateErrorAsync(
        HttpResponseMessage response,
        ReportStateResponse reportState,
        CancellationToken cancellationToken = default)
    {
        string requestLog = await GetFormattedHttpRequestAsync(response.RequestMessage, cancellationToken);

        _logger.LogError(
            "Report status was Error at {ServiceName} sandbox service analyzer."
            + "\nRequest:"
            + "\t\n{Request}"
            + "\nHybrid Analysis Response:"
            + "\n\tError Type: {ErrorType}"
            + "\n\tError Origin: {ErrorOrigin}"
            + "\n\tError Description: {Error}",
            _options.CurrentValue.ServiceName,
            requestLog,
            reportState.ErrorType,
            reportState.ErrorOrigin,
            reportState.Error);
    }
}