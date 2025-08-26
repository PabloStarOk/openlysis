using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;

using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Files.ValueObjects;
using Openlysis.Infrastructure.Configuration;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;

namespace Openlysis.Infrastructure.Services.Files;

/// <summary>
/// Provides context for file storage operations, including processing, uploading, hashing, and removal of files.
/// Implements <see cref="IFileStorageContext"/>.
/// </summary>
internal sealed class FileStorageContext : IFileStorageContext
{
    private readonly IOptions<FileStorageContextOptions> _options;
    private readonly ConcurrentDictionary<string, ProcessedFile> _processedFiles = [];
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IHashService _hashService;
    private readonly ObjectPool<Pipe> _pipePool;
    private readonly MemoryPool<byte> _memoryPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageContext"/> class.
    /// </summary>
    /// <param name="fileStorageProvider">The file storage provider used for file operations.</param>
    /// <param name="options">The options for configuring file storage context behavior.</param>
    /// <param name="hashService">The service used for hashing file data.</param>
    /// <param name="pipePool">The object pool for managing reusable <see cref="Pipe"/> instances.</param>
    /// <param name="memoryPool">The memory pool for efficient buffer management during streaming.</param>
    public FileStorageContext(
        IFileStorageProvider fileStorageProvider,
        IOptions<FileStorageContextOptions> options,
        IHashService hashService,
        ObjectPool<Pipe> pipePool,
        MemoryPool<byte> memoryPool)
    {
        _options = options;
        _fileStorageProvider = fileStorageProvider;
        _hashService = hashService;
        _memoryPool = memoryPool;
        _pipePool = pipePool;
    }

    /// <inheritdoc/>
    public async Task<ProcessedFile> ProcessAsync(
        string fileName,
        string? fileContentType,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (_processedFiles.Count > _options.Value.MaxProcessableFiles)
        {
            throw new InvalidDataException($"The number of attached files must not exceed the limit of {_options.Value.MaxProcessableFiles}.");
        }

        var hashPipe = _pipePool.Get();
        var uploadPipe = _pipePool.Get();

        try
        {
            Task<HashValues> hashTask = _hashService.HashDataAsync(hashPipe.Reader.AsStream(), cancellationToken);
            Task<string> uploadTask = _fileStorageProvider.UploadAsync(uploadPipe.Reader.AsStream(), cancellationToken);
            Task<long> streamingTask = StreamToPipesAsync(fileStream, hashPipe.Writer, uploadPipe.Writer, cancellationToken);

            await Task.WhenAll(uploadTask, hashTask, streamingTask).ConfigureAwait(false);

            long size = await streamingTask;
            string storageFileName = await uploadTask;
            HashValues hashValues = await hashTask;

            await hashPipe.Reader.CompleteAsync();
            await uploadPipe.Reader.CompleteAsync();

            var resolvedContentType = fileContentType ?? _options.Value.FallbackContentType;
            var metadata = new FileMetadata(fileName, resolvedContentType, size);
            var processedFile = new ProcessedFile(metadata, hashValues, storageFileName);
            _processedFiles.TryAdd(storageFileName, processedFile);
            return processedFile;
        }
        catch (Exception exception)
        {
            await hashPipe.Reader.CompleteAsync(exception);
            await uploadPipe.Reader.CompleteAsync(exception);
            throw;
        }
        finally
        {
            _pipePool.Return(hashPipe);
            _pipePool.Return(uploadPipe);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(ProcessedFile processedFile)
    {
        if (!_processedFiles.ContainsKey(processedFile.StorageFileName))
        {
            throw new ArgumentException("Processed file not found in context.", nameof(processedFile));
        }

        _ = _processedFiles.TryRemove(processedFile.StorageFileName, out _);
        await _fileStorageProvider.DeleteAsync(processedFile.StorageFileName);
    }

    /// <inheritdoc/>
    public async Task RemoveAllAsync()
    {
        if (_processedFiles.Values.Count is 0)
        {
            return;
        }

        List<ProcessedFile> filesToRemove = _processedFiles.Values.ToList();
        _processedFiles.Clear();

        await Parallel.ForEachAsync(
            filesToRemove,
            CancellationToken.None,
            async (f, ct) => await _fileStorageProvider.DeleteAsync(f.StorageFileName, ct));
    }

    private async Task<long> StreamToPipesAsync(
        Stream stream,
        PipeWriter hashPipeWriter,
        PipeWriter uploadPipeWriter,
        CancellationToken cancellationToken)
    {
        long totalBytesRead = 0;

        try
        {
            using IMemoryOwner<byte> memoryOwner = _memoryPool.Rent(_options.Value.StreamingBufferSizeBytes);
            Memory<byte> buffer = memoryOwner.Memory;

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                totalBytesRead += bytesRead;
                if (totalBytesRead > _options.Value.MaxFileSizeBytes)
                {
                    throw new InvalidDataException($"File size must not exceed {_options.Value.MaxFileSizeBytes} bytes.");
                }

                var chunk = buffer[..bytesRead];
                await hashPipeWriter.WriteAsync(chunk, cancellationToken);
                await uploadPipeWriter.WriteAsync(chunk, cancellationToken);
            }

            await hashPipeWriter.CompleteAsync();
            await uploadPipeWriter.CompleteAsync();
        }
        catch (Exception exception)
        {
            await hashPipeWriter.CompleteAsync(exception);
            await uploadPipeWriter.CompleteAsync(exception);
            throw;
        }

        return totalBytesRead;
    }
}