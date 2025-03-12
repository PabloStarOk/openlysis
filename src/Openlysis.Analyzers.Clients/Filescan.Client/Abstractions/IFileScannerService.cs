using ErrorOr;

using Filescan.Client.Models.Requests;

using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Filescan.Client.Abstractions;

/// <summary>
/// Defines the client to scan files.
/// </summary>
public interface IFileScannerService
{
    /// <summary>
    /// Uploads a file to be scanned.
    /// </summary>
    /// <param name="scanRequest">Request to scan a file.</param>
    /// <param name="cancellationToken">Cancellation token to stop the operation.</param>
    /// <returns>A <see cref="ServiceAnalysisId"/>.</returns>
    public Task<ErrorOr<ServiceAnalysisId>> UploadAsync(ScanRequest scanRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the scan of a file.
    /// </summary>
    /// <param name="getScanRequest">A <see cref="GetScanRequest"/>.</param>
    /// <param name="cancellationToken">Cancellation token to stop the operation.</param>
    /// <returns>A <see cref="ServiceFileAnalysis"/>.</returns>
    public Task<ErrorOr<ServiceFileAnalysis>> GetScanAsync(GetScanRequest getScanRequest, CancellationToken cancellationToken = default);
}
