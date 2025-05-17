namespace Openlysis.Analyzers.HybridAnalysis.Core.Abstractions.Files;

/// <summary>
/// Interface for detecting file types from stream data.
/// </summary>
internal interface IMimeTypeDetector
{
    /// <summary>
    /// Detects the MIME-type of a file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the file data.</param>
    /// <param name="contentTypeFromRequest">Optional content type hint from request headers.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the detected file type as a string.</returns>
    public Task<string> DetectAsync(
        Stream stream,
        string? contentTypeFromRequest,
        CancellationToken cancellationToken = default);
}