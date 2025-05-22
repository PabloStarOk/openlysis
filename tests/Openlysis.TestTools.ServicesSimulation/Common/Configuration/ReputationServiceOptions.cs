using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for the simulated reputation service.
/// </summary>
internal sealed record ReputationServiceOptions<TStubFactoryOptions>
    : SimulatedServiceOptions<TStubFactoryOptions>
    where TStubFactoryOptions : StubFactoryOptions
{
    /// <summary>
    /// Gets a value indicating whether the simulated reputation service is available.
    /// </summary>
    [Required]
    required public bool IsAvailable { get; init; }

    /// <summary>
    /// Gets the configuration for the endpoint that handles reputation retrieval requests.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    required public SimulatedEndpointOptions GetReputationEndpoint { get; init; }
}