namespace Openlysis.Analyzers.Shared.Core.Configuration;

/// <summary>
/// Represents the options for secrets configuration.
/// </summary>
/// <param name="ApiKey">The API key used for authentication.</param>
public record SecretOptions(string ApiKey);