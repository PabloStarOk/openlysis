namespace Openlysis.Domain.FileAnalyses.Enums;

/// <summary>
/// Represents a verdict.
/// </summary>
public enum Verdict
{
    /// <summary>
    /// Not malicious activity detected.
    /// </summary>
    Undetected,

    /// <summary>
    /// Rare malicious activity detected.
    /// </summary>
    Suspicious,

    /// <summary>
    /// Dangerous malicious activity detected.
    /// </summary>
    Malicious,
}
