using System.Security.Cryptography.X509Certificates;

using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;

using Microsoft.Extensions.Options;

using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Infrastructure.Configuration;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// JWT generator for generating access tokens.
/// </summary>
internal sealed class JwtGenerator : ITokenGenerator
{
    private readonly IOptions<JwtGeneratorOptions> _options;
    private readonly X509Certificate2 _certificate;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtGenerator"/> class.
    /// </summary>
    /// <param name="options">The JWT generator options.</param>
    /// <param name="certificate">The X509 certificate used for signing tokens.</param>
    public JwtGenerator(
        IOptions<JwtGeneratorOptions> options,
        X509Certificate2 certificate)
    {
        _options = options;
        _certificate = certificate;
    }

    /// <inheritdoc/>
    public string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var algorithm = new ES256Algorithm(_certificate);
        var now = DateTimeOffset.UtcNow;
        long nowUnixMilliseconds = now.ToUnixTimeMilliseconds();
        long expirationDateEpoch = now
            .AddSeconds(_options.Value.ExpirationSeconds)
            .ToUnixTimeMilliseconds();

        var token = JwtBuilder
            .Create()
            .WithAlgorithm(algorithm)
            .WithJsonSerializer(new JsonNetSerializer())
            .Issuer(_options.Value.Issuer)
            .Subject(user.Id.ToString())
            .Audience(_options.Value.Audience)
            .ExpirationTime(expirationDateEpoch)
            .NotBefore(nowUnixMilliseconds)
            .IssuedAt(nowUnixMilliseconds)
            .Id(Guid.NewGuid())
            .Encode();

        return token;
    }
}