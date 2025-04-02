using Openlysis.Analyzers.Shared.Core.Configuration;

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
}