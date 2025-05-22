using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for the Analysis Stub Factory.
/// </summary>
/// <remarks>
/// This record inherits from <see cref="StubFactoryOptions"/> and provides
/// specific configuration for analysis-related simulations.
/// </remarks>
internal record AnalysisStubFactoryOptions : StubFactoryOptions
{
    /// <summary>
    /// Gets or initializes a value indicating whether to return a job ID in the response.
    /// </summary>
    required public bool ReturnJobId { get; init; }

    /// <summary>
    /// Gets the configuration for simulating the analysis status.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public AnalysisStatusOptions StatusSimulation { get; init; }

    /// <summary>
    /// Gets the configuration for simulating threat score.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public ThreatScoreOptions ThreatScoreSimulation { get; init; }
}