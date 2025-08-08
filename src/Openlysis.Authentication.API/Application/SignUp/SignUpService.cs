using ErrorOr;

using Openlysis.Authentication.API.Application.Common.Abstractions.Persistence;
using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Application.Common.Errors;
using Openlysis.Domain.Users.Entities;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Authentication.API.Application.SignUp;

/// <summary>
/// Service responsible for handling user sign-up operations.
/// </summary>
internal sealed class SignUpService : ISignUpService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignUpService"/> class.
    /// </summary>
    /// <param name="userRepository">Repository for user persistence operations.</param>
    /// <param name="passwordHasher">Service for hashing user passwords.</param>
    public SignUpService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> SignUpUserAsync(EmailAddress email, string password)
    {
        if (await _userRepository.ExistsAsync(email))
        {
            return AuthError.EmailAlreadyExists;
        }

        byte[] passwordHashSalt = _passwordHasher.GenerateSalt();
        byte[] passwordHash =
            await _passwordHasher.HashAsync(password, passwordHashSalt);
        var user = User.Create(email, passwordHash, passwordHashSalt);
        await _userRepository.AddAsync(user);
        return Result.Success;
    }
}