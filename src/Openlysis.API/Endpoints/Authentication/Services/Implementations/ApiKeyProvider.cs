using System.Security.Cryptography;
using Openlysis.API.Endpoints.Authentication.Services.Interfaces;

namespace Openlysis.API.Endpoints.Authentication.Services.Implementations;

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