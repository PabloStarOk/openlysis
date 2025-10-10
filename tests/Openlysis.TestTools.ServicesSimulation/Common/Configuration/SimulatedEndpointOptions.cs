using System.ComponentModel.DataAnnotations;

namespace Openlysis.TestTools.ServicesSimulation.Common.Configuration;

/// <summary>
/// Represents configuration options for a simulated endpoint in a testing environment.
/// </summary>
internal sealed record SimulatedEndpointOptions
{
    /// <summary>
    /// Gets the simulated latency in milliseconds for the endpoint responses.
    /// </summary>
    [Required]
    required public uint LatencyMs { get; init; }

    /// <summary>
    /// Gets a value indicating whether the endpoint should return an error response.
    /// </summary>
    [Required]
    required public bool ReturnError { get; init; }

    /// <summary>
    /// Gets the rate limit for requests per minute.
    /// </summary>
    required public int MinuteRateLimit { get; init; }

    /// <summary>
    /// Gets the rate limit for requests per hour.
    /// </summary>
    required public int HourRateLimit { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per day.
    /// </summary>
    required public int DailyUsageLimit { get; init; }

    /// <summary>
    /// Gets the maximum number of requests allowed per month.
    /// </summary>
    required public int MonthlyUsageLimit { get; init; }
}