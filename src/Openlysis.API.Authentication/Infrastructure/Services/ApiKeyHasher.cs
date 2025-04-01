using Konscious.Security.Cryptography;

using Openlysis.API.Authentication.Application.Services.Interfaces;

namespace Openlysis.API.Authentication.Infrastructure.Services;

/// <summary>
/// Provides functionality to hash API keys using the Argon2id algorithm.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IApiKeyHasher"/> interface for hashing API keys.
/// </remarks>
public class ApiKeyHasher : IApiKeyHasher
{
    /// <inheritdoc/>
    public async Task<string> HashAsync(string value, CancellationToken cancellationToken = default)
    {
        byte[] apiKeyBytes = Convert.FromHexString(value);

        var argon2Id = new Argon2id(apiKeyBytes);
        argon2Id.MemorySize = 19;
        argon2Id.Iterations = 2;
        argon2Id.DegreeOfParallelism = 1;

        byte[] apiKeyHashBytes = await argon2Id.GetBytesAsync(apiKeyBytes.Length);
        string apiKeyHash = Convert.ToHexString(apiKeyHashBytes);

        return apiKeyHash;
    }
}