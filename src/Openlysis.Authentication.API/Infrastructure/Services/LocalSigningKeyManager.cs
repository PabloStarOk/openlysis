using System.Security.Cryptography.X509Certificates;

using JWT;
using JWT.Algorithms;

using Microsoft.IdentityModel.Tokens;

using Openlysis.Authentication.API.Application.Common.Services;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Provides X.509-based signing key management and JWT algorithm creation.
/// Implements <see cref="IJwkProvider"/> for key retrieval and <see cref="IAlgorithmFactory"/> for algorithm instantiation.
/// </summary>
internal sealed class LocalSigningKeyManager : IJwkProvider, IAlgorithmFactory
{
    private const string Algorithm = "ES256";
    private const string JwkUse = "sig";

    private readonly X509Certificate2 _certificate;
    private readonly IEnumerable<IJwkGenerator> _jwkGenerators;
    private JsonWebKey? _jsonWebKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSigningKeyManager"/> class using the provided X509 certificate and JWK generators.
    /// </summary>
    /// <param name="certificate">The X509 certificate to use for signing and key management.</param>
    /// <param name="jwkGenerators">The collection of JWK generators to use for key creation.</param>
    public LocalSigningKeyManager(
        X509Certificate2 certificate,
        IEnumerable<IJwkGenerator> jwkGenerators)
    {
        _certificate = certificate;
        _jwkGenerators = jwkGenerators;
    }

    /// <inheritdoc/>
    public JsonWebKey GetCurrentSigningKey()
    {
        return _jsonWebKey ??= CreateJsonWebKey();
    }

    /// <inheritdoc/>
    public IEnumerable<JsonWebKey> GetPublicKeys()
    {
        _jsonWebKey ??= CreateJsonWebKey();
        return [_jsonWebKey];
    }

    /// <inheritdoc/>
    public IJwtAlgorithm Create(JwtDecoderContext context)
    {
        return new ES256Algorithm(_certificate);
    }

    private JsonWebKey CreateJsonWebKey()
    {
        var securityKey = new X509SecurityKey(_certificate);
        var generator = _jwkGenerators
            .FirstOrDefault(g => g.CanHandle(securityKey));

        if (generator is null)
        {
            throw new InvalidOperationException("No suitable JWK generator found for the provided certificate.");
        }

        return generator.Generate(securityKey, Algorithm, JwkUse);
    }
}