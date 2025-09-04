using System.Text;

using Microsoft.IO;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Models;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// Provides hashing functionality for messages and their associated files,
/// generating composite hashes using SHA256.
/// </summary>
internal sealed class MessageHashService : IMessageHashService
{
    private readonly IHashService _hashService;
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHashService"/> class.
    /// </summary>
    /// <param name="encoding">The encoding to use for string conversion.</param>
    /// <param name="hashService">The hash service for computing hashes.</param>
    /// <param name="memoryStreamManager">The memory stream manager for efficient memory usage.</param>
    public MessageHashService(
        Encoding encoding,
        IHashService hashService,
        RecyclableMemoryStreamManager memoryStreamManager)
    {
        _encoding = encoding;
        _hashService = hashService;
        _memoryStreamManager = memoryStreamManager;
    }

    /// <inheritdoc/>
    public async ValueTask<HashValues> HashAsync(
        Message message,
        IEnumerable<ProcessedFile> processedFiles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(processedFiles);

        HashValues messageHashValues = HashMessage(message);
        var filesHashValues = processedFiles.Select(f => f.HashValues).ToArray();
        if (filesHashValues.Length is 0)
        {
            return messageHashValues;
        }

        return await CreateCompositeHashAsync(messageHashValues, cancellationToken, filesHashValues);
    }

    private HashValues HashMessage(Message message)
    {
        string concatenatedMessage = string.Join(
            separator: string.Empty,
            message.Sender,
            message.Subject,
            message.Content);

        byte[] inputBytes = _encoding.GetBytes(concatenatedMessage);
        return _hashService.HashData(inputBytes);
    }

    private async Task<HashValues> CreateCompositeHashAsync(
        HashValues messageHashValues,
        CancellationToken cancellationToken,
        params HashValues[] filesHashValues)
    {
        var allHashes = new List<byte[]>(filesHashValues.Length + 1)
        {
            Convert.FromHexString(messageHashValues.Sha256),
        };
        var filesSha256Values = filesHashValues.Select(f => Convert.FromHexString(f.Sha256));
        allHashes.AddRange(filesSha256Values);

        allHashes.Sort((x, y) => x.AsSpan().SequenceCompareTo(y.AsSpan()));

        await using var memoryStream = _memoryStreamManager.GetStream(nameof(CreateCompositeHashAsync));
        foreach (byte[] hash in allHashes)
        {
            await memoryStream.WriteAsync(hash, cancellationToken);
        }

        memoryStream.Position = 0;

        return await _hashService.HashDataAsync(memoryStream, cancellationToken);
    }
}