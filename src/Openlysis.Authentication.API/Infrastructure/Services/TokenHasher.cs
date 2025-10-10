using System.Security.Cryptography;
using System.Text;

using Openlysis.Authentication.API.Application.Common.Abstractions.Services;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Provides hashing functionality for tokens using a specified <see cref="HashAlgorithm"/>.
/// </summary>
internal sealed class TokenHasher : ITokenHasher
{
    /// <summary>
    /// The hash algorithm used for computing token hashes.
    /// </summary>
    private readonly HashAlgorithm _hashAlgorithm;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenHasher"/> class with the specified hash algorithm.
    /// </summary>
    /// <param name="hashAlgorithm">The hash algorithm to use for hashing tokens.</param>
    public TokenHasher(HashAlgorithm hashAlgorithm)
    {
        _hashAlgorithm = hashAlgorithm;
    }

    /// <inheritdoc/>
    public byte[] Hash(string refreshToken)
    {
        var refreshTokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        return _hashAlgorithm.ComputeHash(refreshTokenBytes);
    }
}