using ErrorOr;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenSignInService"/> class.
    /// </summary>
    /// <param name="userRepository">Repository for user data access.</param>
    /// <param name="passwordHasher">Service for password hashing and verification.</param>
    /// <param name="tokenGenerator">Service for generating authentication tokens.</param>
    public TokenSignInService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<string>> SignInAsync(
        EmailAddress email,
        string password)
    {
        bool userExists = await _userRepository.ExistsAsync(email);
        if (!userExists)
        {
            return Error.Unauthorized();
        }

        User user = await _userRepository.GetByEmailAsync(email);
        var passwordsMatch =
            await _passwordHasher.VerifyPasswordAsync(user, password);
        if (!passwordsMatch)
        {
            return Error.Unauthorized();
        }

        return _tokenGenerator.GenerateAccessToken(user);
    }
}