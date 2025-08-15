using Doppler.NET.Models;

namespace Doppler.NET.Abstractions;

/// <summary>
/// A client for interacting with Doppler service.
/// </summary>
public interface IDopplerClient
{
    /// <summary>
    /// Asynchronously retrieves a Doppler secret for the specified project, config, and secret name.
    /// </summary>
    /// <param name="project">The Doppler project name.</param>
    /// <param name="config">The Doppler configuration name.</param>
    /// <param name="secret">The name of the secret to retrieve.</param>
    /// <returns>A <see cref="DopplerSecret"/> if found; otherwise, <c>null</c>.</returns>
    Task<DopplerSecret?> GetSecretAsync(
        string project,
        string config,
        string secret);
}