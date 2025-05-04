using System.Security.Cryptography;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Domain.Common.Entities;

namespace Openlysis.Infrastructure.Services.Hashing;

/// <summary>
/// Service to hash the data of a file.
/// </summary>
public class HashService : IHashService
{
    /// <inheritdoc/>
    public async Task<HashValues> HashDataAsync(Stream data, CancellationToken cancellationToken)
    {
        byte[] md5HashBytes = await MD5.HashDataAsync(data, cancellationToken);
        ResetStreamPosition(data);

        byte[] sha1HashBytes = await SHA1.HashDataAsync(data, cancellationToken);
        ResetStreamPosition(data);

        byte[] sha256HashBytes = await SHA256.HashDataAsync(data, cancellationToken);
        ResetStreamPosition(data);

        byte[] sha512HashBytes = await SHA512.HashDataAsync(data, cancellationToken);

        string md5HashString = Convert.ToHexString(md5HashBytes);
        string sha1HashString = Convert.ToHexString(sha1HashBytes);
        string sha256HashString = Convert.ToHexString(sha256HashBytes);
        string sha512HashString = Convert.ToHexString(sha512HashBytes);

        return new HashValues(
            md5HashString,
            sha1HashString,
            sha256HashString,
            sha512HashString);
    }

    /// <summary>
    /// Resets the position of the stream to the beginning if the stream supports seeking.
    /// </summary>
    /// <param name="stream">The stream to reset.</param>
    private static void ResetStreamPosition(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
    }
}