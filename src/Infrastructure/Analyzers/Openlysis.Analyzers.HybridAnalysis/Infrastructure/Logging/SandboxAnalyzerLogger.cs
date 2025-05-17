using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Common;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;
using Openlysis.Analyzers.Shared.Infrastructure.Logging.Services;

namespace Openlysis.Analyzers.HybridAnalysis.Infrastructure.Logging;

/// <summary>
/// Represents a logger for the sandbox analyzer of the Hybrid Analysis service.
/// </summary>
/// <typeparam name="TCategoryName">
/// The category name type used for logging. Must be a non-nullable type.
/// </typeparam>
/// <remarks>
/// This class extends <see cref="AnalyzerLogger{TCategoryName, HybridAnalyzerOptions}"/>
/// to provide logging functionality specific to the sandbox analyzer.
/// </remarks>
public class SandboxAnalyzerLogger<TCategoryName>
    : AnalyzerLogger<TCategoryName, HybridAnalyzerOptions>
    where TCategoryName : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxAnalyzerLogger{TCategoryName}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The options monitor for <see cref="HybridAnalyzerOptions"/>.</param>
    public SandboxAnalyzerLogger(
        ILogger<TCategoryName> logger,
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

        Logger.LogError(
            "Report status was Error at {ServiceName} sandbox service analyzer."
            + "\nRequest:"
            + "\t\n{Request}"
            + "\nHybrid Analysis Response:"
            + "\n\tError Type: {ErrorType}"
            + "\n\tError Origin: {ErrorOrigin}"
            + "\n\tError Description: {Error}",
            Options.CurrentValue.ServiceName,
            requestLog,
            reportState.ErrorType,
            reportState.ErrorOrigin,
            reportState.Error);
    }
}