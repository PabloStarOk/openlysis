using ErrorOr;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Abstractions;

/// <summary>
/// Defines a base sandbox analyzer.
/// </summary>
public interface ISandboxAnalyzer
{
    /// <summary>
    /// Sends the given request content to a sandbox analysis.
    /// </summary>
    /// <typeparam name="TContent">The type of the content in the request.</typeparam>
    /// <param name="request">The sandbox submission request containing the content to be analyzed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ErrorOr object with the analysis response.</returns>
    public Task<ErrorOr<SandboxSubmitResponse>> AnalyzeAsync<TContent>(
        HttpClient httpClient,
        SandboxSubmitRequest<TContent> request,
        CancellationToken cancellationToken = default)
        where TContent : notnull;

    /// <summary>
    /// Retrieves the status of a report by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the report.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<ErrorOr<Status>> GetReportStatusAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the summary of a report by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the report.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<ErrorOr<SanboxReportSummary>> GetReportSummaryAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default);
}