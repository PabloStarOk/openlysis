namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

/// <summary>
/// Specifies the type of analysis performed in the Hybrid Analysis service.
/// </summary>
internal enum AnalysisType
{
    /// <summary>
    /// A quick scan.
    /// </summary>
    QuickScan,

    /// <summary>
    /// An analysis in an isolated sandbox environment.
    /// </summary>
    Sandbox,
}