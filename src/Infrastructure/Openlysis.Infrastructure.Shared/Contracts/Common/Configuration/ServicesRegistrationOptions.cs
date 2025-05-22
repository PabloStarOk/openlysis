namespace Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

/// <summary>
/// Configuration options for controlling service registration behavior.
/// </summary>
public sealed record ServicesRegistrationOptions
{
    /// <summary>
    /// The configuration section name used to bind these options in the application settings.
    /// </summary>
    public const string SectionName = "ServicesRegistration";

    /// <summary>
    /// Gets a value indicating whether real service implementations should be registered.
    /// </summary>
    required public bool RegisterRealServices { get; init; }

    /// <summary>
    /// Gets a value indicating whether simulated/mock service implementations should be registered.
    /// </summary>
    required public bool RegisterSimulatedServices { get; init; }
}