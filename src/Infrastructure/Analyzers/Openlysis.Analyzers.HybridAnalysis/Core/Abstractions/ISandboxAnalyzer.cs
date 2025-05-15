using ErrorOr;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;

/// <summary>
/// Defines a base sandbox analyzer.
/// </summary>
internal interface ISandboxAnalyzer
{
    /// <summary>
    /// Sends the given request content to a sandbox analysis.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send the request.</param>
    /// <param name="requestFactory">The factory for creating the request to be analyzed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing an <see cref="ErrorOr{T}"/> of <see cref="SandboxSubmitResponse"/>.</returns>
    public Task<ErrorOr<SandboxSubmitResponse>> AnalyzeAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the status of a report by its identifier.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send the request.</param>
    /// <param name="id">The identifier of the report.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Status"/>.</returns>
    public Task<ErrorOr<Status>> GetReportStatusAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the summary of a report by its identifier.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send the request.</param>
    /// <param name="id">The identifier of the report.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing an <see cref="ErrorOr{T}"/> of <see cref="SandboxReportSummary"/>.</returns>
    public Task<ErrorOr<SandboxReportSummary>> GetReportSummaryAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the hash of the given URL according the Hybrid Analysis service.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send the request.</param>
    /// <param name="url">The URL to check the hash for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing an <see cref="ErrorOr{T}"/> of <see cref="string"/>.</returns>
    public Task<ErrorOr<string>> CheckUrlHashAsync(
        HttpClient httpClient,
        Uri url,
        CancellationToken cancellationToken = default);
}