using Openlysis.Application.Common.Models;

namespace Openlysis.Application.Common.Abstractions.Services;

/// <summary>
/// Defines the contract for file storage operations, including processing and removal of files.
/// </summary>
public interface IFileStorageContext
{
    /// <summary>
    /// Processes the provided file stream and returns a <see cref="ProcessedFile"/> representation.
    /// </summary>
    /// <param name="fileName">The name of the file to process.</param>
    /// <param name="fileContentType">The MIME type of the file, if available.</param>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the processed file.</returns>
    public Task<ProcessedFile> ProcessAsync(
        string fileName,
        string? fileContentType,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified processed file by the context.
    /// </summary>
    /// <param name="processedFile">The processed file to remove.</param>
    /// <returns>A task that represents the asynchronous removal operation.</returns>
    public Task RemoveAsync(ProcessedFile processedFile);

    /// <summary>
    /// Removes all processed files by the context.
    /// </summary>
    /// <returns>A task that represents the asynchronous removal operation.</returns>
    public Task RemoveAllAsync();
}