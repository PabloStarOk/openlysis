using System.Security.Cryptography;

using Openlysis.API.Authentication.Application.Services.Interfaces;

namespace Openlysis.API.Authentication.Infrastructure.Services;

/// <summary>
/// Provides functionality to create API keys.
/// </summary>
public class ApiKeyProvider : IApiKeyProvider
{
    /// <inheritdoc/>
    public string Create()
    {
        using var randomGenerator = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[32];
        randomGenerator.GetNonZeroBytes(randomBytes);
        return Convert.ToHexString(randomBytes);
    }
}