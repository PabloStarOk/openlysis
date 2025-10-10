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
    private readonly ConcurrentDictionary<object, Stream> _createdStreams = [];
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
    public async ValueTask<Stream> CreateStreamAsync(object key)
    {
        if (TryGetCachedStream(key, out Stream? cachedStream))
        {
            return cachedStream;
        }

        Stream stream = await _fileStorageProvider
            .DownloadAsync(_fileInstanceId)
            .ConfigureAwait(false);
        _createdStreams.TryAdd(key, stream);
        return stream;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var stream in _createdStreams.Values)
        {
            stream.Dispose();
        }

        _createdStreams.Clear();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (var stream in _createdStreams.Values)
        {
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        _createdStreams.Clear();
    }

    /// <summary>
    /// Attempts to retrieve a cached readable stream from the collection of created streams.
    /// </summary>
    /// <param name="key">The key used to identify the cached stream.</param>
    /// <param name="cachedStream">
    /// When this method returns, contains the cached <see cref="Stream"/> if one is available and readable; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a readable cached stream is found; otherwise, <c>false</c>.
    /// </returns>
    private bool TryGetCachedStream(
        object key,
        [NotNullWhen(true)] out Stream? cachedStream)
    {
        cachedStream = null;
        if (!_createdStreams.TryGetValue(key, out cachedStream))
        {
            return false;
        }

        try
        {
            if (!cachedStream.CanRead)
            {
                return false;
            }

            if (!cachedStream.CanSeek)
            {
                return false;
            }

            cachedStream.Position = 0;
            return true;
        }
        catch (ObjectDisposedException) { }

        return false;
    }
}