using System.ComponentModel.DataAnnotations;

namespace Openlysis.MultiAnalyzer.Configuration;

/// <summary>
/// Options for configuring job timeout behavior.
/// </summary>
internal sealed record JobTimeoutOptions
{
    /// <summary>
    /// The configuration section name for job timeout options.
    /// </summary>
    internal const string SectionName = "JobTimeout";

    /// <summary>
    /// Gets the maximum allowed duration (in seconds) for a job to run before timing out.
    /// </summary>
    [Required]
    [Range(1, uint.MaxValue)]
    required public uint TimeoutSeconds { get; init; }

    /// <summary>
    /// Gets the duration (in seconds) after a timeout to wait before cancelling the job.
    /// </summary>
    [Required]
    [Range(1, uint.MaxValue)]
    required public uint TimeoutCancellationSeconds { get; init; }
}
