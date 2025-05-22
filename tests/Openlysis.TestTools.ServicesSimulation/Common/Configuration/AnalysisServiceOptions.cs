using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Configuration options for simulating an analysis service.
/// </summary>
internal record AnalysisServiceOptions<TStubFactoryOptions>
    : SimulatedServiceOptions<TStubFactoryOptions>
    where TStubFactoryOptions : AnalysisStubFactoryOptions
{
    /// <summary>
    /// Gets the configuration for simulating the duration to finish an analysis process (completed, failed or timed out).
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public AnalysisDurationOptions AnalysisSecondsDuration { get; init; }

    /// <summary>
    /// Gets the configuration for simulating the endpoint behavior to analyze.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public SimulatedEndpointOptions AnalyzeEndpoint { get; init; }

    /// <summary>
    /// Gets the configuration for simulating the status retrieval endpoint behavior.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public SimulatedEndpointOptions GetStatusEndpoint { get; init; }

    /// <summary>
    /// Gets the configuration for simulating the analysis retrieval endpoint behavior.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public SimulatedEndpointOptions GetAnalysisEndpoint { get; init; }
}