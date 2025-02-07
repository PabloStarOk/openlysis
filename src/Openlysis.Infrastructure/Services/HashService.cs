using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.Common.Hash;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// Service to hash the data of a file.
/// </summary>
public class HashService : IHashService
{
    private readonly HashAlgorithm _md5Algorithm;
    private readonly HashAlgorithm _sha1Algorithm;
    private readonly HashAlgorithm _sha256Algorithm;
    private readonly HashAlgorithm _sha512Algorithm;
    private readonly Encoding _encoding;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashService"/> class.
    /// </summary>
    /// <param name="encoding">The encoding to use for converting hash bytes to strings.</param>
    /// <param name="md5Algorithm">The MD5 hash algorithm.</param>
    /// <param name="sha1Algorithm">The SHA1 hash algorithm.</param>
    /// <param name="sha256Algorithm">The SHA256 hash algorithm.</param>
    /// <param name="sha512Algorithm">The SHA512 hash algorithm.</param>
    public HashService(
        Encoding encoding,
        [FromKeyedServices("MD5Algorithm")] HashAlgorithm md5Algorithm,
        [FromKeyedServices("SHA1Algorithm")] HashAlgorithm sha1Algorithm,
        [FromKeyedServices("SHA256Algorithm")] HashAlgorithm sha256Algorithm,
        [FromKeyedServices("SHA512Algorithm")] HashAlgorithm sha512Algorithm)
    {
        _encoding = encoding;
        _md5Algorithm = md5Algorithm;
        _sha1Algorithm = sha1Algorithm;
        _sha256Algorithm = sha256Algorithm;
        _sha512Algorithm = sha512Algorithm;
    }

    /// <inheritdoc/>
    public async Task<HashSet> HashDataAsync(Stream data)
    {
        byte[] md5HashBytes = await _md5Algorithm.ComputeHashAsync(data);
        byte[] sha1HashBytes = await _sha1Algorithm.ComputeHashAsync(data);
        byte[] sha256HashBytes = await _sha256Algorithm.ComputeHashAsync(data);
        byte[] sha512HashBytes = await _sha512Algorithm.ComputeHashAsync(data);

        string md5HashString = _encoding.GetString(md5HashBytes);
        string sha1HashString = _encoding.GetString(sha1HashBytes);
        string sha256HashString = _encoding.GetString(sha256HashBytes);
        string sha512HashString = _encoding.GetString(sha512HashBytes);

        return new HashSet(
            md5HashString,
            sha1HashString,
            sha256HashString,
            sha512HashString);
    }
}