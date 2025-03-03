using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Openlysis.AnalysisWorker.Core.Abstractions;

namespace Openlysis.AnalysisWorker.Infrastructure.Services;

/// <summary>
/// Local storage provider on the current OS.
/// </summary>
public class LocalFileStorageProvider : IFileStorageProvider
{
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

    private static string GetFullTempFilePath(string fileId)
    {
        return Path.Combine(Path.GetTempPath(), fileId);
    }
}