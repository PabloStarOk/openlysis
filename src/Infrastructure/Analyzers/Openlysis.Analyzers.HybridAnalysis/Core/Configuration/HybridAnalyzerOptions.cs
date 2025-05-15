using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Configuration;

/// <summary>
/// Represents the options for the Hybrid Analyzer.
/// </summary>
public record HybridAnalyzerOptions : AnalyzerOptions
{
    /// <summary>
    /// The configuration section name for the Hybrid Analyzer.
    /// </summary>
    public const string SectionName = "HybridAnalysis:Analyzer";

    /// <summary>
    /// Gets the User-Agent required to access the API of Hybrid Analysis.
    /// </summary>
    required public string UserAgent { get; init; }

    /// <summary>
    /// Gets the default environment type.
    /// </summary>
    required public SandboxEnvironment DefaultSandboxEnvironment { get; init; }

    /// <summary>
    /// Gets a value indicating whether experimental anti-evasion techniques should be enabled.
    /// </summary>
    required public bool UseExperimentalAntiEvasion { get; init; }
}