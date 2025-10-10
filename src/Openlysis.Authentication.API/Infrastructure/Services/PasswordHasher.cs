using System.Security.Cryptography;
using System.Text;

using Konscious.Security.Cryptography;

using Microsoft.Extensions.Options;

using Openlysis.Authentication.API.Application.Common.Abstractions.Services;
using Openlysis.Authentication.API.Infrastructure.Configuration;
using Openlysis.Domain.Users.Entities;

namespace Openlysis.Authentication.API.Infrastructure.Services;

/// <summary>
/// Provides password hashing functionality using Argon2id algorithm.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly IOptions<PasswordHasherOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHasher"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options for configuring the password hasher.</param>
    public PasswordHasher(IOptions<PasswordHasherOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(_options.Value.SaltSizeBytes);
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyPasswordAsync(User user, string providedPassword)
    {
        var providedPasswordHash = await HashAsync(
            providedPassword,
            user.PasswordHashSalt);
        return user.PasswordHash.SequenceEqual(providedPasswordHash);
    }

    /// <inheritdoc/>
    public async Task<byte[]> HashAsync(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        var argon2Id = new Argon2id(passwordBytes)
        {
            Salt = salt.ToArray(),
            MemorySize = _options.Value.MemorySizeKibibytes,
            Iterations = _options.Value.Iterations,
            DegreeOfParallelism = _options.Value.ParallelismDegree,
        };
        return await argon2Id.GetBytesAsync(_options.Value.HashSizeBytes);
    }
}