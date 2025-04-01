namespace Openlysis.Domain.Common.Enums;

/// <summary>
/// Dangerous zone of a report.
/// </summary>
public enum ThreatZone
{
    /// <summary>
    /// The threat zone is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Scan didn't detect anything.
    /// </summary>
    Green,

    /// <summary>
    /// Scan detected the file/url as suspicious.
    /// </summary>
    Yellow,

    /// <summary>
    /// Scan detected the file/url as dangerous.
    /// </summary>
    Red,
}
