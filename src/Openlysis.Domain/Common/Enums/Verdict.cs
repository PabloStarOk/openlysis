namespace Openlysis.Domain.Common.Enums;

/// <summary>
/// Represents a verdict.
/// </summary>
public enum Verdict
{
    /// <summary>
    /// The verdict is unknown.
    /// </summary>
    Unknown,

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
