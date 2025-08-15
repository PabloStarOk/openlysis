namespace Doppler.NET.Models;

/// <summary>
/// A Doppler secret value with its raw and computed forms, and an optional note.
/// </summary>
/// <param name="Raw">The raw secret value as retrieved.</param>
/// <param name="Computed">The computed or processed secret value.</param>
/// <param name="Note">An optional note describing the secret.</param>
public sealed record DopplerSecretValue(
    string Raw,
    string Computed,
    string Note);