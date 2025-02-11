using System.Security.Cryptography;

using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.Common.Hash;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// Service to hash the data of a file.
/// </summary>
public class HashService : IHashService
{
    private readonly MD5 _md5Algorithm;
    private readonly SHA1 _sha1Algorithm;
    private readonly SHA256 _sha256Algorithm;
    private readonly SHA512 _sha512Algorithm;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashService"/> class.
    /// </summary>
    /// <param name="md5Algorithm">A <see cref="MD5"/> hash algorithm.</param>
    /// <param name="sha1Algorithm">The <see cref="SHA1"/> hash algorithm.</param>
    /// <param name="sha256Algorithm">The <see cref="SHA256"/> hash algorithm.</param>
    /// <param name="sha512Algorithm">The <see cref="SHA512"/> hash algorithm.</param>
    public HashService(
        MD5 md5Algorithm,
        SHA1 sha1Algorithm,
        SHA256 sha256Algorithm,
        SHA512 sha512Algorithm)
    {
        _md5Algorithm = md5Algorithm;
        _sha1Algorithm = sha1Algorithm;
        _sha256Algorithm = sha256Algorithm;
        _sha512Algorithm = sha512Algorithm;
    }

    /// <inheritdoc/>
    public async Task<ContentHashSet> HashDataAsync(Stream data)
    {
        byte[] md5HashBytes = await _md5Algorithm.ComputeHashAsync(data);
        ResetStreamPosition(data);

        byte[] sha1HashBytes = await _sha1Algorithm.ComputeHashAsync(data);
        ResetStreamPosition(data);

        byte[] sha256HashBytes = await _sha256Algorithm.ComputeHashAsync(data);
        ResetStreamPosition(data);

        byte[] sha512HashBytes = await _sha512Algorithm.ComputeHashAsync(data);

        string md5HashString = Convert.ToHexString(md5HashBytes);
        string sha1HashString = Convert.ToHexString(sha1HashBytes);
        string sha256HashString = Convert.ToHexString(sha256HashBytes);
        string sha512HashString = Convert.ToHexString(sha512HashBytes);

        return new ContentHashSet(
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