namespace Openlysis.Domain.Common.Reports;

/// <summary>
/// Dangerous zone of a report.
/// </summary>
public enum ReportZone
{
    /// <summary>
    /// There's no report zone.
    /// </summary>
    None,

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
