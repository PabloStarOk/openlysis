using ErrorOr;

using Openlysis.Analyzers.Filescan.Core.Models.Requests;
using Openlysis.Analyzers.Filescan.Core.Models.Responses;

namespace Openlysis.Analyzers.Filescan.Core.Abstractions;

/// <summary>
/// Defines the client to scan files.
/// </summary>
public interface IFilescanAnalyzer
{
    /// <summary>
    /// Analyzes a file or URL asynchronously.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestFactory">The factory to create the request.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ErrorOr object with a <see cref="ScanResponse"/>.</returns>
    public Task<ErrorOr<ScanResponse>> AnalyzeAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the analysis result asynchronously.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="getScanRequest">The request object containing the scan details.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ErrorOr object with a <see cref="GetAnalysisResponse"/>.</returns>
    public Task<ErrorOr<GetAnalysisResponse>> GetAnalysisAsync(
        HttpClient httpClient,
        GetScanRequest getScanRequest,
        CancellationToken cancellationToken = default);
}
