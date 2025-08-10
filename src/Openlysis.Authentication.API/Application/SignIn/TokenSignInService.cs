using ErrorOr;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Application.Common.Models;
using Openlysis.Authentication.API.Application.Common.Services;
using Openlysis.Domain.Users.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Application.SignIn;

/// <summary>
/// Service responsible for handling user sign-in using tokens.
/// </summary>
internal sealed class TokenSignInService : ITokenSignInService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly RefreshTokenStore _refreshTokenStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenSignInService"/> class.
    /// </summary>
    /// <param name="userRepository">Repository for user data access.</param>
    /// <param name="passwordHasher">Service for password hashing and verification.</param>
    /// <param name="tokenGenerator">Service for generating authentication tokens.</param>
    /// <param name="refreshTokenStore">Store for managing refresh tokens.</param>
    public TokenSignInService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        RefreshTokenStore refreshTokenStore)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _refreshTokenStore = refreshTokenStore;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<AuthTokens>> SignInAsync(
        EmailAddress email,
        string password)
    {
        User? user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return Error.Unauthorized();
        }

        var passwordsMatch =
            await _passwordHasher.VerifyPasswordAsync(user, password);

        if (!passwordsMatch)
        {
            return Error.Unauthorized();
        }

        AuthTokens authTokens = _tokenGenerator.Generate(user);
        await _refreshTokenStore.AddAsync(user.Id, authTokens.RefreshToken);
        return authTokens;
    }
}