using System.ComponentModel.DataAnnotations;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Represents the weights for different types of sensors.
/// </summary>
public record SensorWeightsOptions
{
    /// <summary>
    /// Gets the weight for Intrusion Detection System sensors.
    /// </summary>
    /// <remarks>
    /// The value must be between 1 and 5.
    /// </remarks>
    [Range(1, 5, ErrorMessage = "Value for {0} must be between {1} or {2}")]
    required public int Ids { get; init; }

    /// <summary>
    /// Gets the weight for Threat Detection System sensors.
    /// </summary>
    /// <remarks>
    /// The value must be between 1 and 5.
    /// </remarks>
    [Range(1, 5, ErrorMessage = "Value for {0} must be between {1} or {2}")]
    required public int Tds { get; init; }

    /// <summary>
    /// Gets the weight for URL Query analyzer.
    /// </summary>
    /// <remarks>
    /// The value must be between 1 and 5.
    /// </remarks>
    [Range(1, 5, ErrorMessage = "Value for {0} must be between {1} or {2}")]
    required public int Urlquery { get; init; }
}