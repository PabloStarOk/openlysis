using System.ComponentModel.DataAnnotations;

namespace Openlysis.Infrastructure.Configuration;

/// <summary>
/// Options for configuring analysis reuse behavior.
/// </summary>
internal sealed record AnalysisReuseOptions
{
    /// <summary>
    /// The configuration section name for analysis reuse options.
    /// </summary>
    public const string SectionName = "AnalysisReuse";

    /// <summary>
    /// Gets the maximum age (in hours) for which analysis results can be reused.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxAgeHours { get; init; }
}