using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Reports;

namespace Openlysis.Domain.FileAnalyses.ValueObjects;

/// <summary>
/// Information of a detection.
/// </summary>
/// <param name="IsEmpty">True if the detection exists, otherwise false.</param>
/// <param name="Type">Type of the detection if it is a virus.</param>
/// <param name="Zone">Dangerous zone.</param>
public sealed record DetectionInfo(bool IsEmpty, string Type, ThreatZone Zone)
{
    /// <summary>
    /// Gets the representation of an empty detection info.
    /// </summary>
    public static DetectionInfo Empty { get; } = new (
        false, string.Empty, ThreatZone.None);
}
