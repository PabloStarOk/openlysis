namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

/// <summary>
/// Represents the verdict of a hybrid analysis.
/// </summary>
public enum HybridVerdict
{
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