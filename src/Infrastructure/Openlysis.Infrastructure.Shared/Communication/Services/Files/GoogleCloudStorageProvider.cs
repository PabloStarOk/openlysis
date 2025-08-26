using System.IO.Pipelines;

using Google.Cloud.Storage.V1;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;

namespace Openlysis.Infrastructure.Shared.Communication.Services.Files;

/// <summary>
/// Provides Google Cloud Storage operations for file upload, download, and deletion.
/// Implements <see cref="IFileStorageProvider"/>.
/// </summary>
internal sealed class GoogleCloudStorageProvider : IFileStorageProvider
{
    private readonly ILogger<GoogleCloudStorageProvider> _logger;
    private readonly IOptions<GoogleCloudStorageOptions> _options;
    private readonly ObjectPool<Pipe> _pipePool;
    private readonly Lazy<StorageClient> _storageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleCloudStorageProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger for logging operations.</param>
    /// <param name="options">Options containing Google Cloud Storage configuration.</param>
    /// <param name="pipePool">Object pool for <see cref="Pipe"/> instances.</param>
    /// <param name="storageClient">Lazy-loaded Google Cloud <see cref="StorageClient"/>.</param>
    public GoogleCloudStorageProvider(
        ILogger<GoogleCloudStorageProvider> logger,
        IOptions<GoogleCloudStorageOptions> options,
        ObjectPool<Pipe> pipePool,
        Lazy<StorageClient> storageClient)
    {
        _logger = logger;
        _options = options;
        _pipePool = pipePool;
        _storageClient = storageClient;
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(Stream fileData, CancellationToken cancellationToken = default)
    {
        string bucketName = _options.Value.BucketName;
        var fileId = GlobalId.CreateUnique().ToString();
        await _storageClient.Value
            .UploadObjectAsync(bucketName, fileId, null, fileData, null, cancellationToken)
            .ConfigureAwait(false);
        return fileId;
    }

    /// <inheritdoc/>
    public Task<Stream> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        Pipe pipe = _pipePool.Get();
        var pooledPipeStream = PooledPipeStream.Create(_pipePool, pipe);
        _ = Task.Run(() => StreamFileAsync(fileId, pooledPipeStream, cancellationToken), cancellationToken);
        return Task.FromResult<Stream>(pooledPipeStream);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        string bucketName = _options.Value.BucketName;
        await _storageClient.Value
            .DeleteObjectAsync(bucketName, fileId, null, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task StreamFileAsync(
        string fileId,
        PooledPipeStream pooledPipeStream,
        CancellationToken cancellationToken = default)
    {
        string bucketName = _options.Value.BucketName;
        _logger.LogInformation("Starting GCS download stream for file: {FileId}", fileId);

        try
        {
            await _storageClient.Value
                .DownloadObjectAsync(bucketName, fileId, pooledPipeStream, null, cancellationToken)
                .ConfigureAwait(false);
            await pooledPipeStream.CompleteWriterAsync();
            _logger.LogInformation("Finished GCS download stream for file: {FileId}", fileId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during GCS download stream for file: {FileId}", fileId);
            await pooledPipeStream.CompleteWriterAsync(exception);
        }
    }

    /// <summary>
    /// A pooled stream implementation using <see cref="System.IO.Pipelines.Pipe"/> for efficient memory usage.
    /// Used to facilitate streaming data between Google Cloud Storage and consumers.
    /// </summary>
    private sealed class PooledPipeStream : Stream
    {
        public override bool CanRead => _pipeReaderStream.CanRead;

        public override bool CanSeek => _pipeReaderStream.CanSeek;

        public override bool CanWrite => _pipeWriterStream.CanWrite;

        public override long Length => _pipeReaderStream.Length;

        public override long Position
        {
            get => _pipeReaderStream.Position;
            set => _pipeReaderStream.Position = value;
        }

        private readonly ObjectPool<Pipe> _pipePool;
        private readonly Pipe _pipe;
        private readonly Stream _pipeWriterStream;
        private readonly Stream _pipeReaderStream;
        private int _disposed;

        private PooledPipeStream(
            ObjectPool<Pipe> pipePool,
            Pipe pipe,
            Stream pipeWriterStream,
            Stream pipeReaderStream)
        {
            _pipePool = pipePool;
            _pipe = pipe;
            _pipeWriterStream = pipeWriterStream;
            _pipeReaderStream = pipeReaderStream;
        }

        public static PooledPipeStream Create(ObjectPool<Pipe> pipePool, Pipe pipe)
        {
            return new PooledPipeStream(
                pipePool,
                pipe,
                pipe.Writer.AsStream(),
                pipe.Reader.AsStream());
        }

        public override void Flush()
        {
            _pipeReaderStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _pipeReaderStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _pipeReaderStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _pipeReaderStream.ReadAsync(buffer, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _pipeReaderStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _pipeReaderStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _pipeWriterStream.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _pipeWriterStream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _pipeWriterStream.WriteAsync(buffer, cancellationToken);
        }

        public async ValueTask CompleteWriterAsync(Exception? exception = null)
        {
            await _pipe.Writer.CompleteAsync(exception);
        }

        public override async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) is not 0)
            {
                return;
            }

            await base.DisposeAsync();
            await _pipeWriterStream.DisposeAsync();
            await _pipe.Writer.CompleteAsync();
            await _pipeReaderStream.DisposeAsync();
            await _pipe.Reader.CompleteAsync();
            _pipePool.Return(_pipe);
        }

        protected override void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) is not 0)
            {
                return;
            }

            base.Dispose(disposing);
            _pipeWriterStream.Dispose();
            _pipe.Writer.Complete();
            _pipeReaderStream.Dispose();
            _pipe.Reader.Complete();
            _pipePool.Return(_pipe);
        }
    }
}