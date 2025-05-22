using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using Openlysis.TestTools.ServicesSimulation.Common.Configuration;

namespace Openlysis.TestTools.ServicesSimulation.Files.Configuration;

/// <summary>
/// Represents configuration options for file report stub simulations.
/// </summary>
internal sealed record FileReportStubOptions
{
    /// <summary>
    /// Gets the configuration for verdict simulation.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public VerdictOptions VerdictSimulation { get; init; }

    /// <summary>
    /// Gets the configuration for threat score simulation.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public ThreatScoreOptions ThreatScoreSimulation { get; init; }
}