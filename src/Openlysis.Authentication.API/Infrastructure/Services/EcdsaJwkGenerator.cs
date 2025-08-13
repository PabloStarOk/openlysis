using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.Tokens;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Generates JSON Web Keys (JWK) for ECDSA-based <see cref="SecurityKey"/> instances.
/// </summary>
internal sealed class EcdsaJwkGenerator : IJwkGenerator
{
    /// <inheritdoc/>
    public bool CanHandle(SecurityKey key)
    {
        if (key is ECDsaSecurityKey)
        {
            return true;
        }

        if (key is not X509SecurityKey x509Key)
        {
            return false;
        }

        using var ecdsa = x509Key.Certificate.GetECDsaPublicKey();
        return ecdsa is not null;
    }

    /// <inheritdoc/>
    public JsonWebKey Generate(
        SecurityKey key,
        string algorithm,
        string use)
    {
        return key switch
        {
            ECDsaSecurityKey ecdsaKey => GenerateFromEcdsaKey(ecdsaKey, algorithm, use),
            X509SecurityKey x509Key => GenerateFromX509Key(x509Key, algorithm, use),
            _ => throw new NotSupportedException($"The key type '{key.GetType().Name}' is not supported by {nameof(EcdsaJwkGenerator)}.")
        };
    }

    private static JsonWebKey GenerateFromEcdsaKey(
        ECDsaSecurityKey key,
        string algorithm,
        string use)
    {
        ECDsa ecdsa = key.ECDsa;
        string kid = key.KeyId;
        var ecParameters = ecdsa.ExportParameters(false);

        var jwk = new JsonWebKey
        {
            Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
            Use = use,
            Kid = kid,
            Alg = algorithm,
            Crv = GetCurveName(ecParameters.Curve),
            X = Base64UrlEncoder.Encode(ecParameters.Q.X),
            Y = Base64UrlEncoder.Encode(ecParameters.Q.Y),
        };

        return jwk;
    }

    private static JsonWebKey GenerateFromX509Key(
        X509SecurityKey key,
        string algorithm,
        string use)
    {
        using ECDsa? ecdsa = key.Certificate.GetECDsaPublicKey();
        string kid = key.Certificate.Thumbprint;
        byte[] rawCertData = key.Certificate.RawData;

        if (ecdsa is null)
        {
            throw new InvalidOperationException("Security key does not contain a valid ECDSA public key.");
        }

        var ecParameters = ecdsa.ExportParameters(false);

        var jwk = new JsonWebKey
        {
            Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
            Use = use,
            Kid = kid,
            Alg = algorithm,
            Crv = GetCurveName(ecParameters.Curve),
            X = Base64UrlEncoder.Encode(ecParameters.Q.X),
            Y = Base64UrlEncoder.Encode(ecParameters.Q.Y),
        };

        var x5C = Convert.ToBase64String(rawCertData);
        jwk.X5c.Add(x5C);

        return jwk;
    }

    private static string GetCurveName(ECCurve curve)
    {
        return curve.Oid.FriendlyName switch
        {
            "nistP256" or "ECDSA_P256" => JsonWebKeyECTypes.P256,
            "nistP384" or "ECDSA_P384" => JsonWebKeyECTypes.P384,
            "nistP521" or "ECDSA_P521" => JsonWebKeyECTypes.P521,
            _ => throw new NotSupportedException($"The curve '{curve.Oid.FriendlyName}' is not a JWA-supported curve.")
        };
    }
}