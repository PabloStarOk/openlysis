namespace Openlysis.API.Endpoints.Authentication.Services.Interfaces;

/// <summary>
/// Interface for hashing API Keys..
/// </summary>
public interface IApiKeyHasher
{
    /// <summary>
    /// Asynchronously hashes the provided value.
    /// </summary>
    /// <param name="value">The value to hash.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The hashed value as a string.</returns>
    public Task<string> HashAsync(string value, CancellationToken cancellationToken = default);
}