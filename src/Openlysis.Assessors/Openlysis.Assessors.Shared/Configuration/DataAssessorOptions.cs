using System.ComponentModel.DataAnnotations;

namespace Openlysis.Assessors.Shared.Configuration;

/// <summary>
/// Defines base options required to configure an assessor.
/// </summary>
/// <remarks>
/// It must be implemented by concrete assessor services to know the name of the configuration section.
/// </remarks>
public abstract record DataAssessorOptions
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    required public string ServiceName { get; init; }

    /// <summary>
    /// Gets the base address of the service.
    /// </summary>
    [Url]
    required public Uri BaseAddress { get; init; }

    /// <summary>
    /// Gets the timeout for requests in milliseconds.
    /// </summary>
    required public int RequestsTimeoutMs { get; init; }
}