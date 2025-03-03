using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Openlysis.MultiAnalyzer.Core.Abstractions;

namespace Openlysis.MultiAnalyzer.Infrastructure.Services;

/// <summary>
/// Local storage provider on the current OS.
/// </summary>
public class LocalFileStorageProvider : IFileStorageProvider
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
        string tempFilePath = GetFullTempFilePath(fileId.ToString());

        var fileStreamOptions = new FileStreamOptions
        {
            Access = FileAccess.Write,
            Mode = FileMode.CreateNew,
            Options = FileOptions.Asynchronous,
        };

        if (OperatingSystem.IsLinux())
        {
            fileStreamOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        await using var fileStream = new FileStream(tempFilePath, fileStreamOptions);
        fileData.Position = 0;
        await fileData.CopyToAsync(fileStream, cancellationToken);
        return fileId.ToString();
    }

    /// <inheritdoc/>
    public Task<Stream> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        string tempFilePath = GetFullTempFilePath(fileId);
        var fileStreamOptions = new FileStreamOptions
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous,
        };

        Stream fileData = new FileStream(tempFilePath, fileStreamOptions);
        return Task.FromResult(fileData);
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        string tempFilePath = GetFullTempFilePath(fileId);
        File.Delete(tempFilePath);
        return Task.CompletedTask;
    }

    private string GetFullTempFilePath(string fileId)
    {
        return Path.Combine(_tempSubdirectory.FullName, fileId);
    }
}