using System.Security.Cryptography;

using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using NodaTime;

using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Application.Common.Models;
using Openlysis.Authentication.API.Application.Common.Services;
using Openlysis.Authentication.API.Infrastructure.Configuration;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// JWT generator for generating access tokens.
/// </summary>
internal sealed class JwtGenerator : ITokenGenerator
{
    private readonly IOptions<JwtGeneratorOptions> _jwtOptions;
    private readonly IOptions<RefreshTokenOptions> _refreshTokenOptions;
    private readonly IClock _clock;
    private readonly IJwkProvider _jwkProvider;
    private readonly IAlgorithmFactory _algorithmFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtGenerator"/> class.
    /// </summary>
    /// <param name="jwtOptions">The JWT generator options.</param>
    /// <param name="refreshTokenOptions">The refresh token options.</param>
    /// <param name="clock">The clock used for time operations.</param>
    /// <param name="jwkProvider">The provider for the signing key.</param>
    /// <param name="algorithmFactory">The factory for JWT algorithms.</param>
    public JwtGenerator(
        IOptions<JwtGeneratorOptions> jwtOptions,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        IClock clock,
        IJwkProvider jwkProvider,
        IAlgorithmFactory algorithmFactory)
    {
        _jwtOptions = jwtOptions;
        _refreshTokenOptions = refreshTokenOptions;
        _clock = clock;
        _jwkProvider = jwkProvider;
        _algorithmFactory = algorithmFactory;
    }

    /// <inheritdoc/>
    public AuthTokens Generate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        string accessToken = GenerateAccessToken(user);
        string refreshToken = GenerateRefreshToken();
        Instant refreshTokenExpiration = GetRefreshTokenExpiration();
        return new AuthTokens(accessToken, refreshToken, refreshTokenExpiration);
    }

    private string GenerateAccessToken(User user)
    {
        JsonWebKey signingKey = _jwkProvider.GetCurrentSigningKey();
        Instant now = _clock.GetCurrentInstant();

        Instant expirationDate =
            now + Duration.FromSeconds(_jwtOptions.Value.ExpirationSeconds);

        long nowUnixSeconds = now.ToUnixTimeSeconds();
        long expirationUnixSeconds = expirationDate.ToUnixTimeSeconds();

        var token = JwtBuilder
            .Create()
            .WithAlgorithmFactory(_algorithmFactory)
            .WithJsonSerializer(new JsonNetSerializer())
            .Issuer(_jwtOptions.Value.Issuer)
            .Subject(user.Id.ToString())
            .Audience(_jwtOptions.Value.Audience)
            .ExpirationTime(expirationUnixSeconds)
            .NotBefore(nowUnixSeconds)
            .IssuedAt(nowUnixSeconds)
            .Id(Guid.NewGuid())
            .AddHeader(HeaderName.KeyId, signingKey.Kid)
            .Encode();

        return token;
    }

    private string GenerateRefreshToken()
    {
        int tokenSizeBytes = _jwtOptions.Value.RefreshTokenSizeBytes;
        byte[] refreshToken = RandomNumberGenerator.GetBytes(tokenSizeBytes);
        return Convert.ToBase64String(refreshToken);
    }

    private Instant GetRefreshTokenExpiration()
    {
        var expirationDays = _refreshTokenOptions.Value.ExpirationDays;
        Instant now = _clock.GetCurrentInstant();
        return now + Duration.FromDays(expirationDays);
    }
}