using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Openlysis.Analyzers.Shared.Infrastructure.Files.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;

namespace Openlysis.Analyzers.Shared.Infrastructure.Files.Services;

/// <summary>
/// Provides a factory for creating file streams using a specified file instance identifier and storage provider.
/// </summary>
public sealed class FileStreamFactory : IStreamFactory, IDisposable, IAsyncDisposable
{
    private readonly ConcurrentBag<Stream> _createdStreams = [];
    private readonly string _fileInstanceId;
    private readonly IFileStorageProvider _fileStorageProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStreamFactory"/> class.
    /// </summary>
    /// <param name="fileInstanceId">The unique identifier for the file instance.</param>
    /// <param name="fileStorageProvider">The file storage provider used to access file streams.</param>
    public FileStreamFactory(
        string fileInstanceId,
        IFileStorageProvider fileStorageProvider)
    {
        _fileInstanceId = fileInstanceId;
        _fileStorageProvider = fileStorageProvider;
    }

    /// <inheritdoc/>
    public async ValueTask<Stream> CreateStreamAsync()
    {
        if (TryGetCachedStream(out Stream? cachedStream))
        {
            return cachedStream;
        }

        Stream stream = await _fileStorageProvider.DownloadAsync(_fileInstanceId);
        _createdStreams.Add(stream);
        return stream;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var stream in _createdStreams)
        {
            stream.Dispose();
        }

        _createdStreams.Clear();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (var stream in _createdStreams)
        {
            await stream.DisposeAsync();
        }

        _createdStreams.Clear();
    }

    /// <summary>
    /// Attempts to retrieve a cached readable stream from the collection of created streams.
    /// </summary>
    /// <param name="cachedStream">
    /// When this method returns, contains the cached <see cref="Stream"/> if one is available and readable; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a readable cached stream is found; otherwise, <c>false</c>.
    /// </returns>
    private bool TryGetCachedStream([NotNullWhen(true)] out Stream? cachedStream)
    {
        cachedStream = null;
        foreach (var stream in _createdStreams)
        {
            try
            {
                if (!stream.CanRead)
                {
                    continue;
                }

                stream.Position = 0;
                cachedStream = stream;
                return true;
            }
            catch (ObjectDisposedException) { }
        }

        return false;
    }
}