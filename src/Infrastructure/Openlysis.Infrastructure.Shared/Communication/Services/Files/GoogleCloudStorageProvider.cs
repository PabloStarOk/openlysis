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
    private readonly IOptions<GoogleCloudStorageOptions> _options;
    private readonly ObjectPool<Pipe> _pipePool;
    private readonly Lazy<StorageClient> _storageClient;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleCloudStorageProvider"/> class.
    /// </summary>
    /// <param name="options">Options containing Google Cloud Storage configuration.</param>
    /// <param name="pipePool">Object pool for <see cref="Pipe"/> instances.</param>
    /// <param name="storageClient">Lazy-loaded Google Cloud <see cref="StorageClient"/>.</param>
    /// <param name="loggerFactory">Factory for creating logger instances.</param>
    public GoogleCloudStorageProvider(
        IOptions<GoogleCloudStorageOptions> options,
        ObjectPool<Pipe> pipePool,
        Lazy<StorageClient> storageClient,
        ILoggerFactory loggerFactory)
    {
        _options = options;
        _pipePool = pipePool;
        _storageClient = storageClient;
        _loggerFactory = loggerFactory;
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
        ILogger<PooledDownloadStream> logger = _loggerFactory.CreateLogger<PooledDownloadStream>();
        var pooledPipeStream = PooledDownloadStream.Create(logger, _pipePool, _storageClient.Value, pipe);
        pooledPipeStream.StartStreaming(_options.Value.BucketName, fileId, cancellationToken);
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

    /// <summary>
    /// A pooled stream implementation using <see cref="System.IO.Pipelines.Pipe"/> for efficient memory usage.
    /// Used to facilitate streaming data between Google Cloud Storage and consumers.
    /// </summary>
    private sealed class PooledDownloadStream : Stream
    {
        public override bool CanRead => _pipeReaderStream.CanRead;

        public override bool CanSeek => _pipeReaderStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _pipeReaderStream.Length;

        public override long Position
        {
            get => _pipeReaderStream.Position;
            set => _pipeReaderStream.Position = value;
        }

        private readonly ILogger<PooledDownloadStream> _logger;
        private readonly ObjectPool<Pipe> _pipePool;
        private readonly StorageClient _storageClient;
        private readonly Pipe _pipe;
        private readonly Stream _pipeWriterStream;
        private readonly Stream _pipeReaderStream;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _streamingTask;
        private int _disposed;

        private PooledDownloadStream(
            ILogger<PooledDownloadStream> logger,
            ObjectPool<Pipe> pipePool,
            StorageClient storageClient,
            Pipe pipe,
            Stream pipeWriterStream,
            Stream pipeReaderStream)
        {
            _logger = logger;
            _pipePool = pipePool;
            _storageClient = storageClient;
            _pipe = pipe;
            _pipeWriterStream = pipeWriterStream;
            _pipeReaderStream = pipeReaderStream;
        }

        public static PooledDownloadStream Create(
            ILogger<PooledDownloadStream> logger,
            ObjectPool<Pipe> pipePool,
            StorageClient storageClient,
            Pipe pipe)
        {
            return new PooledDownloadStream(
                logger,
                pipePool,
                storageClient,
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
            throw new NotSupportedException();
        }

        public void StartStreaming(string bucketName, string storageFileName, CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _streamingTask = StreamAsync(bucketName, storageFileName, _cancellationTokenSource.Token);
        }

        public override async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) is not 0)
            {
                return;
            }

            if (_cancellationTokenSource is not null)
            {
                await _cancellationTokenSource.CancelAsync();
            }

            try
            {
                if (_streamingTask is not null)
                {
                    await _streamingTask;
                }
            }
            catch (Exception)
            {
                // Already logged
            }

            await base.DisposeAsync();
            _streamingTask?.Dispose();
            _cancellationTokenSource?.Dispose();
            await _pipe.Writer.CompleteAsync();
            await _pipe.Reader.CompleteAsync();
            await _pipeWriterStream.DisposeAsync();
            await _pipeReaderStream.DisposeAsync();
            _pipePool.Return(_pipe);
        }

        protected override void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) is not 0)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();

            try
            {
                _streamingTask?.GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // Already logged
            }

            base.Dispose(disposing);
            _streamingTask?.Dispose();
            _cancellationTokenSource?.Dispose();
            _pipe.Writer.Complete();
            _pipe.Reader.Complete();
            _pipeWriterStream.Dispose();
            _pipeReaderStream.Dispose();
            _pipePool.Return(_pipe);
        }

        /// <summary>
        /// Streams the contents of a file from Google Cloud Storage into the pipe writer stream.
        /// </summary>
        /// <param name="bucketName">The name of the GCS bucket.</param>
        /// <param name="storageFileName">The name of the file in GCS to download.</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        private async Task StreamAsync(string bucketName, string storageFileName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting GCS download stream for file: {FileName}", storageFileName);
            try
            {
                await _storageClient
                    .DownloadObjectAsync(bucketName, storageFileName, _pipeWriterStream, null, cancellationToken)
                    .ConfigureAwait(false);
                await _pipe.Writer.CompleteAsync();
                _logger.LogInformation("Finished GCS download stream for file: {FileName}", storageFileName);
            }
            catch (OperationCanceledException)
            {
                await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error during GCS download stream for file: {FileName}", storageFileName);
                await _pipe.Writer.CompleteAsync(exception).ConfigureAwait(false);
            }
        }
    }
}