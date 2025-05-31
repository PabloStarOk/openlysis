using System.ComponentModel.DataAnnotations;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Analyzers.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Configuration.Common;

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
    [Required]
    required public string UserAgent { get; init; }

    /// <summary>
    /// Gets the default environment type to be used in URL sandbox analyses.
    /// </summary>
    required public SandboxEnvironment DefaultSandboxEnvironment { get; init; }

    /// <summary>
    /// Gets the preferred service to use for quick scanning operations.
    /// </summary>
    [Required]
    required public string PreferredQuickScanServiceName { get; init; }

    /// <summary>
    /// Gets the maximum allowed size of a file in bytes that can be analyzed.
    /// </summary>
    [Range(1, int.MaxValue)]
    required public int FileMaxSizeInBytes { get; init; }

    /// <summary>
    /// Gets a value indicating whether experimental anti-evasion techniques should be enabled.
    /// </summary>
    required public bool UseExperimentalAntiEvasion { get; init; }
}