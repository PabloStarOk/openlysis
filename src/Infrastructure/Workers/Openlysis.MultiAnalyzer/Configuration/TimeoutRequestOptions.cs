using System.ComponentModel.DataAnnotations;

namespace Openlysis.MultiAnalyzer.Configuration;

/// <summary>
/// Options for configuring a timeout request.
/// </summary>
internal sealed record TimeoutRequestOptions
{
    /// <summary>
    /// The configuration section name for the timeout request options.
    /// </summary>
    internal const string SectionName = "TimeoutRequest";

    /// <summary>
    /// Gets the timeout in seconds for processing a timeout request.
    /// </summary>
    [Required]
    [Range(1, uint.MaxValue)]
    required public uint SecondsTimeout { get; init; }
}
