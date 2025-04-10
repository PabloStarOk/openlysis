using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Domain.Files.Entities;

/// <summary>
/// Represents a single report.
/// </summary>
public class Report : Entity<ReportId>
{
    /// <summary>
    /// Gets the verdict of the scan.
    /// </summary>
    public Verdict Verdict { get; }

    /// <summary>
    /// Gets the threat zone of the scan.
    /// </summary>
    public ThreatZone ThreatZone { get; }

    /// <summary>
    /// Gets the threat level of the scan.
    /// </summary>
    public float? ThreatLevel { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Report"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the report.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatLevel">The threat level of the scan. Optional.</param>
    protected Report(
        ReportId id,
        Verdict verdict,
        ThreatZone threatZone,
        float? threatLevel = null)
        : base(id)
    {
        Verdict = verdict;
        ThreatZone = threatZone;
        ThreatLevel = threatLevel;
        ThreatLevel = threatLevel;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    protected Report()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="Report"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the report.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatLevel">The threat level of the scan. Optional.</param>
    /// <returns>A new instance of the <see cref="Report"/> class.</returns>
    public static Report Create(
        string id,
        Verdict verdict,
        ThreatZone threatZone,
        float? threatLevel)
    {
        return new Report(
            ReportId.Create(id),
            verdict,
            threatZone,
            threatLevel);
    }
}
