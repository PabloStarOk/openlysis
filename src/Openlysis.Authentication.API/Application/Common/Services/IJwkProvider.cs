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
    /// Retrieves the collection of public keys in JSON Web Key (JWK) format.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{JsonWebKey}"/> containing public keys.</returns>
    public IEnumerable<JsonWebKey> GetPublicKeys();
}