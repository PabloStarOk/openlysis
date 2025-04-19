namespace Openlysis.Infrastructure.Configuration;

/// <summary>
/// Configuration for regular expressions.
/// </summary>
internal record RegexSettings
{
    /// <summary>
    /// The configuration section name for regex timeout settings.
    /// </summary>
    public const string SectionName = "Regex";

    /// <summary>
    /// Gets the timeout value for regex operations, in milliseconds.
    /// </summary>
    required public int TimeoutMs { get; init; }
}