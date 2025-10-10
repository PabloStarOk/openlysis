namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

/// <summary>
/// Represents the verdict of a hybrid analysis.
/// </summary>
public enum HybridAnalysisVerdict
{
    /// <summary>
    /// The verdict is unknown or could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// No verdict has been determined or no analysis was performed.
    /// </summary>
    NoVerdict,

    /// <summary>
    /// No specific threat detected.
    /// </summary>
    NoSpecificThreat,

    /// <summary>
    /// The result is suspicious.
    /// </summary>
    Suspicious,

    /// <summary>
    /// The result is malicious.
    /// </summary>
    Malicious,
}