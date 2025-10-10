using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// An abstract record representing stub factory configuration options.
/// Serves as a base class for specific stub factory configurations.
/// </summary>
internal abstract record StubFactoryOptions
{
    /// <summary>
    /// Gets or initializes the configuration for verdict simulation.
    /// This property is required but can be null.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public VerdictOptions VerdictSimulation { get; init; }
}