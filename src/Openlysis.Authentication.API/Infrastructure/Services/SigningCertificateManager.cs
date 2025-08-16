using JWT;
using JWT.Algorithms;

using Microsoft.IdentityModel.Tokens;

using Openlysis.Authentication.API.Application.Common.Services;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Provides X.509-based signing key management and JWT algorithm creation.
/// Implements <see cref="IJwkProvider"/> for key retrieval and <see cref="IAlgorithmFactory"/> for algorithm instantiation.
/// </summary>
internal sealed class SigningCertificateManager : IJwkProvider, IAlgorithmFactory
{
    private const string Algorithm = "ES256";
    private const string JwkUse = "sig";

    private readonly ICertificateProvider _certificateProvider;
    private readonly IEnumerable<IJwkGenerator> _jwkGenerators;
    private JsonWebKey? _jsonWebKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="SigningCertificateManager"/> class using the provided certificate provider, JWK generators.
    /// </summary>
    /// <param name="certificateProvider">The certificate provider to use for signing and key management.</param>
    /// <param name="jwkGenerators">The collection of JWK generators to use for key creation.</param>
    public SigningCertificateManager(
        ICertificateProvider certificateProvider,
        IEnumerable<IJwkGenerator> jwkGenerators)
    {
        _certificateProvider = certificateProvider;
        _jwkGenerators = jwkGenerators;
    }

    /// <inheritdoc/>
    public JsonWebKey GetCurrentSigningKey()
    {
        return _jsonWebKey ??= CreateJsonWebKey();
    }

    /// <inheritdoc/>
    public JsonWebKeySet GetJsonWebKeySet()
    {
        _jsonWebKey ??= CreateJsonWebKey();
        var jwks = new JsonWebKeySet
        {
            Keys = { _jsonWebKey },
        };
        return jwks;
    }

    /// <inheritdoc/>
    public IJwtAlgorithm Create(JwtDecoderContext context)
    {
        return new ES256Algorithm(_certificateProvider.Certificate);
    }

    private JsonWebKey CreateJsonWebKey()
    {
        var securityKey = new X509SecurityKey(_certificateProvider.Certificate);
        var generator = _jwkGenerators
            .FirstOrDefault(g => g.CanHandle(securityKey));

        if (generator is null)
        {
            throw new InvalidOperationException("No suitable JWK generator found for the provided certificate.");
        }

        return generator.Generate(securityKey, Algorithm, JwkUse);
    }
}