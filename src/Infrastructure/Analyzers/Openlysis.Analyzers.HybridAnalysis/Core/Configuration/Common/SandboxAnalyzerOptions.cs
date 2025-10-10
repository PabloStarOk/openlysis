using System.ComponentModel.DataAnnotations;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Common;

/// <summary>
/// Configuration options for the sandbox analyzer that defines file type support across different operating systems.
/// </summary>
/// <remarks>
/// Contains collections of file types categorized by their compatibility with specific operating systems.
/// The <see cref="AllSupportedFiles"/> property provides a union of all supported file types.
/// </remarks>
internal record SandboxAnalyzerOptions
{
    /// <summary>
    /// The configuration section name for sandbox analyzer options in appSettings.
    /// </summary>
    internal const string SectionName = "HybridAnalysis:SandboxAnalyzer";

    /// <summary>
    /// Gets a combined collection of all supported file types across different operating systems.
    /// </summary>
    /// <remarks>
    /// This property combines file types from AnyOsSupportedFiles and all OS-specific collections
    /// (Windows, Windows 7 HWP, Linux, Mac, and Android) into a single set.
    /// </remarks>
    internal IReadOnlySet<string> AllSupportedFiles =>
        AnyOsSupportedMimeTypes
            .Union(WinOnlySupportedMimeTypes)
            .Union(Win7HwpOnlySupportedMimeTypes)
            .Union(LinuxOnlySupportedMimeTypes)
            .Union(MacOnlySupportedMimeTypes)
            .Union(AndroidOnlySupportedMimeTypes)
            .ToHashSet();

    /// <summary>
    /// Gets or initializes file extensions supported on any operating system.
    /// </summary>
    [MinLength(1)]
    required public IReadOnlySet<string> AnyOsSupportedMimeTypes { get; init; }

    /// <summary>
    /// Gets or initializes file extensions exclusively supported on Windows operating systems.
    /// </summary>
    [MinLength(1)]
    required public IReadOnlySet<string> WinOnlySupportedMimeTypes { get; init; }

    /// <summary>
    /// Gets or initializes file extensions exclusively supported on Windows 7 with HWP (Hangul Word Processor) support.
    /// </summary>
    [MinLength(1)]
    required public IReadOnlySet<string> Win7HwpOnlySupportedMimeTypes { get; init; }

    /// <summary>
    /// Gets or initializes file extensions exclusively supported on Linux operating systems.
    /// </summary>
    [MinLength(1)]
    required public IReadOnlySet<string> LinuxOnlySupportedMimeTypes { get; init; }

    /// <summary>
    /// Gets or initializes file extensions exclusively supported on macOS operating systems.
    /// </summary>
    [MinLength(1)]
    required public IReadOnlySet<string> MacOnlySupportedMimeTypes { get; init; }

    /// <summary>
    /// Gets or initializes file extensions exclusively supported on Android environments.
    /// </summary>
    [MinLength(1)]
    required public IReadOnlySet<string> AndroidOnlySupportedMimeTypes { get; init; }
}