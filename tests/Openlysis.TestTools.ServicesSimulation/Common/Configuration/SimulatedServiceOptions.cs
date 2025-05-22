using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for a simulated service.
/// </summary>
/// <remarks>
/// This class provides configuration for simulated services including
/// its identification, verdict behavior, completion duration, and response latency.
/// </remarks>
internal abstract record SimulatedServiceOptions<TStubFactoryOptions>
    where TStubFactoryOptions : StubFactoryOptions
{
    /// <summary>
    /// Gets the name of the simulated service.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(1)]
    required public string Name { get; init; }

    /// <summary>
    /// Gets the stub factory options for the simulated service.
    /// </summary>
    /// <remarks>
    /// The stub factory is responsible for creating and configuring service stubs.
    /// </remarks>
    [Required]
    [ValidateObjectMembers]
    required public TStubFactoryOptions StubFactory { get; init; }
}