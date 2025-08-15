namespace Doppler.NET.Models;

/// <summary>
/// A Doppler secret with a name and value.
/// </summary>
/// <param name="Name">The name of the secret.</param>
/// <param name="Value">The value of the secret.</param>
public sealed record DopplerSecret(
    string Name,
    DopplerSecretValue Value);