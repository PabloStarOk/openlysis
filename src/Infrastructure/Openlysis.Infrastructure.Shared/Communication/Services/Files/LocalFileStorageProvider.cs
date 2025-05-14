using Openlysis.Infrastructure.Shared.Communication.Abstractions;

namespace Openlysis.Infrastructure.Shared.Communication.Services.Files;

/// <summary>
/// Local storage provider on the current OS.
/// </summary>
internal class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly DirectoryInfo _tempSubdirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileStorageProvider"/> class.
    /// </summary>
    /// <param name="tempSubdirectory">The temporary subdirectory for storing files.</param>
    public LocalFileStorageProvider(
        DirectoryInfo tempSubdirectory)
    {
        _tempSubdirectory = tempSubdirectory;
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(Stream fileData, CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid();
        await using var fileStream = CreateFileStream(fileId.ToString(), FileAccess.Write, FileMode.CreateNew);
        fileData.Position = 0;
        await fileData.CopyToAsync(fileStream, cancellationToken);
        return fileId.ToString();
    }

    /// <inheritdoc/>
    public Task<Stream> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        Stream fileData = CreateFileStream(fileId, FileAccess.Read, FileMode.Open);
        return Task.FromResult(fileData);
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        string tempFilePath = GetFullTempFilePath(fileId);
        File.Delete(tempFilePath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the full temporary file path for the given file ID.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <returns>The full temporary file path.</returns>
    private string GetFullTempFilePath(string fileId)
    {
        return Path.Combine(_tempSubdirectory.FullName, fileId);
    }

    /// <summary>
    /// Creates a new <see cref="FileStream"/> with the specified parameters.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <param name="fileAccess">The file access mode.</param>
    /// <param name="fileMode">The file mode.</param>
    /// <returns>A new <see cref="FileStream"/>.</returns>
    private FileStream CreateFileStream(string fileId, FileAccess fileAccess, FileMode fileMode)
    {
        var fileStreamOptions = new FileStreamOptions
        {
            Access = fileAccess,
            Mode = fileMode,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        };

        if (OperatingSystem.IsLinux()
            && fileMode is FileMode.CreateNew or FileMode.Create)
        {
            fileStreamOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        return new FileStream(GetFullTempFilePath(fileId), fileStreamOptions);
    }
}