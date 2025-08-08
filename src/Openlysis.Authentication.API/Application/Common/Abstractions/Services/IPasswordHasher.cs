namespace Openlysis.Authentication.API.Application.Common.Abstractions.Services;

/// <summary>
/// Provides methods for generating salts and hashing passwords.
/// </summary>
internal interface IPasswordHasher
{
    /// <summary>
    /// Generates a cryptographically secure random salt.
    /// </summary>
    /// <returns>A byte array representing the generated salt.</returns>
    public byte[] GenerateSalt();

    /// <summary>
    /// Hashes the specified password using the provided salt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="salt">The salt to use in the hashing process.</param>
    /// <returns>The hashed password as a byte array.</returns>
    public Task<byte[]> HashAsync(string password, byte[] salt);
}
