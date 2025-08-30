using System.ComponentModel.DataAnnotations;

namespace Openlysis.Infrastructure.Shared.Communication.Configuration;

/// <summary>
/// Represents configuration for a message consumer.
/// </summary>
public sealed record ConsumerConfig
{
    private static int[] DefaultRetryIntervals { get; } = [250, 500, 1000, 5000, 10000];

    /// <summary>
    /// Gets the name of the consumer.
    /// </summary>
    [Required]
    [MinLength(1)]
    required public string Name { get; init; }

    /// <summary>
    /// Gets the maximum number of concurrent operations allowed for the consumer.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int ConcurrencyLimit { get; init; }

    /// <summary>
    /// Gets the intervals (in milliseconds) to wait between retry attempts.
    /// </summary>
    public int[] RetryIntervals { get; init; } = DefaultRetryIntervals;
}