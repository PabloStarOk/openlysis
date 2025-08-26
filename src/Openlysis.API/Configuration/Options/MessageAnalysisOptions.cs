namespace Openlysis.API.Configuration.Options;

/// <summary>
/// Options for configuring message analysis options.
/// </summary>
public record MessageAnalysisOptions
{
    /// <summary>
    /// The configuration section name for file upload options.
    /// </summary>
    public const string SectionName = "MessageAnalysis";

    /// <summary>
    /// Gets the maximum number of attached files allowed per message.
    /// </summary>
    required public int MaxAttachedFiles { get; init; }
}