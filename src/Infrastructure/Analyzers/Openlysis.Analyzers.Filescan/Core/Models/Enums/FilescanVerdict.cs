namespace Openlysis.Analyzers.Filescan.Core.Models.Enums;

/// <summary>
/// Represents the possible verdicts of Filescan services.
/// </summary>
public enum FilescanVerdict
{
    /// <summary>
    /// The verdict is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// No threat detected.
    /// </summary>
    NoThreat,

    /// <summary>
    /// The file is benign.
    /// </summary>
    Benign,

    /// <summary>
    /// The file is suspicious.
    /// </summary>
    Suspicious,

    /// <summary>
    /// The file is likely malicious.
    /// </summary>
    LikelyMalicious,

    /// <summary>
    /// The file is malicious.
    /// </summary>
    Malicious,
}