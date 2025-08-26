namespace Openlysis.Infrastructure.Shared.Communication.Abstractions;

/// <summary>
/// Defines a contract for file storage providers.
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>
    /// Uploads a file.
    /// </summary>
    /// <param name="fileData">Data of the file to load.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="string"/> representing the name of the file in the storage.</returns>
    public Task<string> UploadAsync(Stream fileData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file.
    /// </summary>
    /// <param name="fileName">A <see cref="string"/> representing the name of the file.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Stream"/>.</returns>
    public Task<Stream> DownloadAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="fileName">A <see cref="string"/> representing the name of the file.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}