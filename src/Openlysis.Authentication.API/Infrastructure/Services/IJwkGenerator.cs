using Microsoft.IdentityModel.Tokens;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Defines a JSON Web Keys (JWK) generator from <see cref="SecurityKey"/> instances.
/// </summary>
internal interface IJwkGenerator
{
    /// <summary>
    /// Determines whether the specified <see cref="SecurityKey"/> can be handled by this generator.
    /// </summary>
    /// <param name="key">The security key to check.</param>
    /// <returns><c>true</c> if the key can be handled; otherwise, <c>false</c>.</returns>
    public bool CanHandle(SecurityKey key);

    /// <summary>
    /// Generates a <see cref="JsonWebKey"/> from the provided <see cref="SecurityKey"/>.
    /// </summary>
    /// <param name="key">The security key to convert.</param>
    /// <param name="algorithm">The algorithm to associate with the JWK.</param>
    /// <param name="use">The intended use of the JWK (e.g., "sig" or "enc").</param>
    /// <returns>A <see cref="JsonWebKey"/> representation of the key.</returns>
    public JsonWebKey Generate(
        SecurityKey key,
        string algorithm,
        string use);
}