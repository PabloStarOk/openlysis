using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Openlysis.MultiAnalyzer.Configuration;

/// <summary>
/// Options for configuring the options for multi-analyzer objects.
/// </summary>
internal sealed record MultiAnalyzerOptions
{
    /// <summary>
    /// The configuration section name for MultiAnalyzer options.
    /// </summary>
    public const string SectionName = "MultiAnalyzer";

    /// <summary>
    /// Gets the list of retry intervals (in seconds) to start analyses.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<int> RetryIntervals { get; init; }
}