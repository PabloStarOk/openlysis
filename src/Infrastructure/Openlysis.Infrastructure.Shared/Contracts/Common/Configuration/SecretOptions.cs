using System.ComponentModel.DataAnnotations;

namespace Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

/// <summary>
/// Represents the options for secrets configuration.
/// </summary>
/// <remarks>
/// This options must be implemented by concrete records to provide a name for the section configuration.
/// </remarks>
public abstract record SecretOptions
{
    /// <summary>
    /// Gets the API key for the secrets' configuration.
    /// </summary>
    [Required]
    required public string ApiKey { get; init; }
}