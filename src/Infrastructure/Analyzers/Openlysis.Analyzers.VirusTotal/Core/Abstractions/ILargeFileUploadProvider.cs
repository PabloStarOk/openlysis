using ErrorOr;

namespace Openlysis.Analyzers.VirusTotal.Core.Abstractions;

/// <summary>
/// Provides functionality for obtaining URLs for large file uploads.
/// </summary>
internal interface ILargeFileUploadProvider
{
    /// <summary>
    /// Retrieves a URL that can be used to upload large files.
    /// </summary>
    /// <remarks>
    /// This method is designed for handling files that exceed standard upload limits
    /// and need special upload mechanisms.
    /// </remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains an <see cref="ErrorOr{T}"/> of string which will either contain:
    /// - The upload URL on success.
    /// - An error describing why retrieval failed.
    /// </returns>
    public Task<ErrorOr<string>> GetFileUploadUrlAsync(
        CancellationToken cancellationToken = default);
}