using ErrorOr;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Objects;
using Openlysis.Analyzers.HybridAnalysis.Core.Models.Responses;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Common;

/// <summary>
/// Interface for performing quick scanning operations on files using Hybrid Analysis.
/// </summary>
internal interface IQuickScanner
{
    /// <summary>
    /// Retrieves the current state of the quick scan service.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make API requests.</param>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A response containing the quick scan state information on success, or an error.</returns>
    public Task<ErrorOr<QuickScanService[]>> GetStateAsync(
        HttpClient httpClient,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a new scan of a file or URL.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make API requests.</param>
    /// <param name="requestFactory">Factory for creating appropriate HTTP requests.</param>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A response containing the scan ID on success, or an error.</returns>
    public Task<ErrorOr<QuickScanResponse>> ScanAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the status of a previously initiated scan.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make API requests.</param>
    /// <param name="id">The ID of the scan to retrieve.</param>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A response containing scan status information on success, or an error.</returns>
    public Task<ErrorOr<QuickScanResponse>> GetQuickScanAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the analysis summary for a file or URL by its SHA256 hash.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make API requests.</param>
    /// <param name="sha256">The SHA256 hash of the analyzed file or URL.</param>
    /// <param name="cancellationToken">Token for cancelling the operation.</param>
    /// <returns>A response containing the analysis verdict and threat score on success, or an error.</returns>
    public Task<ErrorOr<AnalysisSummary>> GetAnalysisSummaryAsync(
        HttpClient httpClient,
        string sha256,
        CancellationToken cancellationToken = default);
}