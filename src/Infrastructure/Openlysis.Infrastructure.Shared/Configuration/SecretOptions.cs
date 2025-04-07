namespace Openlysis.Infrastructure.Shared.Configuration;

/// <summary>
/// Represents the options for secrets configuration.
/// </summary>
/// <param name="ApiKey">The API key used for authentication.</param>
/// <remarks>
/// This options must be implemented by concrete records to provide a name for the section configuration.
/// </remarks>
public abstract record SecretOptions(string ApiKey);