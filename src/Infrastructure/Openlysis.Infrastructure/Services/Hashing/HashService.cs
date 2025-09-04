using System.Buffers;
using System.Security.Cryptography;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Domain.Common.Entities;

namespace Openlysis.Infrastructure.Services.Hashing;

/// <summary>
/// Service to calculate the <see cref="HashValues"/> of a <see cref="Stream"/>.
/// </summary>
internal class HashService : IHashService
{
    private const int BufferSize = 81_920;
    private readonly MemoryPool<byte> _memoryPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashService"/> class.
    /// </summary>
    /// <param name="memoryPool">The memory pool used for buffer management.</param>
    public HashService(MemoryPool<byte> memoryPool)
    {
        _memoryPool = memoryPool;
    }

    /// <inheritdoc/>
    public async Task<HashValues> HashDataAsync(Stream data, CancellationToken cancellationToken)
    {
        using var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);

        using IMemoryOwner<byte> memoryOwner = _memoryPool.Rent(BufferSize);
        var buffer = memoryOwner.Memory;

        int bytesRead;
        while ((bytesRead = await data.ReadAsync(buffer, cancellationToken)) > 0)
        {
            md5.AppendData(buffer.Span[..bytesRead]);
            sha1.AppendData(buffer.Span[..bytesRead]);
            sha256.AppendData(buffer.Span[..bytesRead]);
            sha512.AppendData(buffer.Span[..bytesRead]);
        }

        string hexMd5 = Convert.ToHexString(md5.GetHashAndReset());
        string hexSha1 = Convert.ToHexString(sha1.GetHashAndReset());
        string hexSha256 = Convert.ToHexString(sha256.GetHashAndReset());
        string hexSha512 = Convert.ToHexString(sha512.GetHashAndReset());

        return HashValues.Create(
            hexMd5,
            hexSha1,
            hexSha256,
            hexSha512);
    }

    /// <inheritdoc/>
    public HashValues HashData(ReadOnlySpan<byte> data)
    {
        string hexMd5 = Convert.ToHexString(MD5.HashData(data));
        string hexSha1 = Convert.ToHexString(SHA1.HashData(data));
        string hexSha256 = Convert.ToHexString(SHA256.HashData(data));
        string hexSha512 = Convert.ToHexString(SHA512.HashData(data));

        return HashValues.Create(
            hexMd5,
            hexSha1,
            hexSha256,
            hexSha512);
    }
}