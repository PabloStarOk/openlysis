using Microsoft.IdentityModel.Tokens;

namespace Openlysis.Authentication.API.Application.Common.Services;

/// <summary>
/// Provides signing and public key retrieval functionality for token operations.
/// </summary>
internal interface IJwkProvider
{
    /// <summary>
    /// Retrieves the current signing key used for token signing operations.
    /// </summary>
    /// <returns>The current <see cref="JsonWebKey"/> used for signing.</returns>
    public JsonWebKey GetCurrentSigningKey();

    /// <summary>
    /// Retrieves the set of public JSON Web Keys (JWKs) for token validation.
    /// </summary>
    /// <returns>A <see cref="JsonWebKeySet"/> containing the public keys.</returns>
    public JsonWebKeySet GetJsonWebKeySet();
}