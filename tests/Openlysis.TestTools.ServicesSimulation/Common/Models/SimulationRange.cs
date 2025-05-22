using System.ComponentModel.DataAnnotations;

namespace Openlysis.TestTools.ServicesSimulation.Common.Models;

/// <summary>
/// Represents a range with minimum and maximum values.
/// </summary>
/// <typeparam name="TValue">The type of values in the range. Must be non-null.</typeparam>
internal sealed record SimulationRange<TValue>
    where TValue : notnull
{
    /// <summary>
    /// Gets the minimum value of the range.
    /// </summary>
    [Required]
    required public TValue Minimum { get; init; }

    /// <summary>
    /// Gets the maximum value of the range.
    /// </summary>
    [Required]
    required public TValue Maximum { get; init; }
}