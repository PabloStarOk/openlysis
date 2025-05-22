namespace Openlysis.TestTools.ServicesSimulation.Common.Enums;

/// <summary>
/// Specifies the type of simulation to be used in simulated services.
/// </summary>
internal enum SimulationType
{
    /// <summary>
    /// Represents a completely random value with no constraints.
    /// </summary>
    Random,

    /// <summary>
    /// Represents a fixed, predetermined value.
    /// </summary>
    Fixed,

    /// <summary>
    /// Represents a random value within a specific range of values.
    /// </summary>
    Range,

    /// <summary>
    /// Represents a value from a predefined set of values.
    /// </summary>
    Set,
}