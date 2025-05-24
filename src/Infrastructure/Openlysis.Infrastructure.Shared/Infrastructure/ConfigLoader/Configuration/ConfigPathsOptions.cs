namespace Openlysis.Infrastructure.Shared.Infrastructure.ConfigLoader.Configuration;

/// <summary>
/// Represents configuration paths for directories and files.
/// </summary>
internal sealed record ConfigPathsOptions
{
    /// <summary>
    /// The name of the configuration section that contains the configuration paths options.
    /// </summary>
    internal const string SectionName = "ConfigurationPaths";

    /// <summary>
    /// Gets a collection of directory URIs to be used for configuration loading.
    /// </summary>
    required public IReadOnlyList<string> Directories { get; init; } = [];

    /// <summary>
    /// Gets a collection of file URIs to be used for configuration loading.
    /// </summary>
    required public IReadOnlyList<string> JsonFiles { get; init; } = [];
}