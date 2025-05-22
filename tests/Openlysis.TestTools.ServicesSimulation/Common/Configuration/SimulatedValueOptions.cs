using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Enums;
using Openlysis.TestTools.ServicesSimulation.Common.Models;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Abstract base class for configuring simulated value options.
/// </summary>
/// <typeparam name="TValue">The type of value being simulated. Must be non-null.</typeparam>
internal abstract record SimulatedValueOptions<TValue>
    where TValue : notnull
{
    /// <summary>
    /// Gets the type of simulation to be performed.
    /// </summary>
    [Required]
    required public SimulationType SimulationType { get; init; }

    /// <summary>
    /// Gets the fixed value to be used when the simulation type is set to Fixed.
    /// </summary>
    public TValue? FixedValue { get; init; }

    /// <summary>
    /// Gets a range of values to be used when the simulation type is set to Range.
    /// </summary>
    [ValidateObjectMembers]
    public SimulationRange<TValue>? ValuesRange { get; init; }

    /// <summary>
    /// Gets the set of concrete values to be used when the simulation type is set to Set.
    /// Defaults to an empty HashSet.
    /// </summary>
    public HashSet<TValue> ValuesSet { get; init; } = [];

    /// <summary>
    /// Gets the type name for use in validation error messages.
    /// This property should return a descriptive name of the specific value type being simulated.
    /// </summary>
    protected abstract string TypeName { get; }

    /// <summary>
    /// Validates the configuration options based on the specified simulation type.
    /// </summary>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating whether the validation passed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the simulation type is not supported.</exception>
    internal ValidateOptionsResult Validate()
    {
        return SimulationType switch
        {
            SimulationType.Random => ValidateAsRandom(),
            SimulationType.Fixed => ValidateAsFixed(),
            SimulationType.Range => ValidateAsRange(),
            SimulationType.Set => ValidateAsSet(),
            _ => throw new InvalidOperationException("Cannot validate simulated value options."),
        };
    }

    /// <summary>
    /// Validates the options when the simulation type is set to Random.
    /// </summary>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating whether the validation passed.</returns>
    protected abstract ValidateOptionsResult ValidateAsRandom();

    /// <summary>
    /// Validates the options when the simulation type is set to Fixed.
    /// </summary>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating whether the validation passed.</returns>
    protected abstract ValidateOptionsResult ValidateAsFixed();

    /// <summary>
    /// Validates the options when the simulation type is set to Range.
    /// </summary>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating whether the validation passed.</returns>
    protected abstract ValidateOptionsResult ValidateAsRange();

    /// <summary>
    /// Validates the options when the simulation type is set to Set.
    /// </summary>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating whether the validation passed.</returns>
    protected abstract ValidateOptionsResult ValidateAsSet();

    /// <summary>
    /// Creates a formatted validation failure result with the type name prefix.
    /// </summary>
    /// <param name="message">The validation failure message.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> representing the validation failure.</returns>
    protected ValidateOptionsResult FormattedFail(string message)
    {
        return ValidateOptionsResult.Fail($"{TypeName}: {message}");
    }
}