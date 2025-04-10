using System.ComponentModel.DataAnnotations;

namespace Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

/// <summary>
/// Defines a base type for options of services.
/// </summary>
/// <remarks>
/// This options must be implemented to provide a name for the section configuration.
/// </remarks>
public abstract record ServiceOptions
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    [Required]
    required public string ServiceName { get; init; }

    /// <summary>
    /// Gets the base address of the service.
    /// </summary>
    [Required]
    required public Uri BaseAddress { get; init; }

    /// <summary>
    /// Gets the timeout for requests in milliseconds.
    /// </summary>
    [Required]
    required public int RequestsTimeoutMs { get; init; }
}