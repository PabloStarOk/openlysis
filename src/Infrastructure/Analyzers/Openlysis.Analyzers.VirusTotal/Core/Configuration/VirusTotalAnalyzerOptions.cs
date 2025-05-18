using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.VirusTotal.Core.Configuration;

/// <summary>
/// Options for configuring the VirusTotal analyzer.
/// </summary>
public record VirusTotalAnalyzerOptions : AnalyzerOptions
{
    /// <summary>
    /// The configuration section name for the VirusTotal analyzer.
    /// </summary>
    public const string SectionName = "VirusTotal:Analyzer";

    /// <summary>
    /// Gets the maximum allowed size of a file in bytes that can be analyzed.
    /// </summary>
    required public int FileMaxSizeInBytes { get; init; }
}