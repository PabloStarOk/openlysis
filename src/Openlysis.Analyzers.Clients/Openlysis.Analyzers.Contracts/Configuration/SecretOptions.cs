namespace Openlysis.Analyzers.Contracts.Configuration;

/// <summary>
/// Represents the options for secrets configuration.
/// </summary>
/// <param name="ApiKey">The API key used for authentication.</param>
public record SecretOptions(string ApiKey);