using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.Filescan.Core.Configuration;

/// <summary>
/// Options for configuring the Filescan Analyzer.
/// </summary>
public record FilescanAnalyzerOptions : AnalyzerOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "Filescan:Analyzer";

    /// <summary>
    /// Gets the maximum allowed size of a file in bytes that can be analyzed.
    /// </summary>
    required public int FileMaxSizeInBytes { get; init; }
}