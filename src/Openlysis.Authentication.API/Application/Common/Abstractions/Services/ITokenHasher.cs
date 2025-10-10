namespace Openlysis.Authentication.API.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a contract for hashing tokens.
/// </summary>
internal interface ITokenHasher
{
    /// <summary>
    /// Hashes the provided token and returns the hash as a byte array.
    /// </summary>
    /// <param name="token">The token to hash.</param>
    /// <returns>The hashed token as a byte array.</returns>
    public byte[] Hash(string token);
}