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
}