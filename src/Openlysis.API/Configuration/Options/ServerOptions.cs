namespace Openlysis.API.Configuration.Options;

/// <summary>
/// Server configuration options.
/// </summary>
public record ServerOptions
{
    /// <summary>
    /// The name of the configuration section for server options.
    /// </summary>
    public const string SectionName = "Server";

    /// <summary>
    /// Gets the maximum size of the request body in bytes.
    /// </summary>
    required public int MaxRequestBodySize { get; init; }
}