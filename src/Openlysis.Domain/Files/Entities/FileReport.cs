using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Domain.Files.Entities;

/// <summary>
/// Report of an analysis for a file.
/// </summary>
public class FileReport : Entity<ReportId>
{
    /// <summary>
    /// Gets the verdict of the scan.
    /// </summary>
    public Verdict Verdict { get; private set; }

    /// <summary>
    /// Gets the threat zone of the scan.
    /// </summary>
    public ThreatZone ThreatZone { get; private set; }

    /// <summary>
    /// Gets the threat score of the file.
    /// </summary>
    public ThreatScore ThreatScore { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileReport"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the report.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatScore">The threat score of the file.</param>
    private FileReport(
        ReportId id,
        Verdict verdict,
        ThreatZone threatZone,
        ThreatScore threatScore)
        : base(id)
    {
        Verdict = verdict;
        ThreatZone = threatZone;
        ThreatScore = threatScore;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private FileReport()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="FileReport"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the report.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatScore">The threat score of the file.</param>
    /// <returns>A new instance of the <see cref="FileReport"/> class with a normalized threat score.</returns>
    public static FileReport Create(
        string id,
        Verdict verdict,
        ThreatZone threatZone,
        ThreatScore? threatScore = null)
    {
        return new FileReport(
            ReportId.Create(id),
            verdict,
            threatZone,
            threatScore ?? ThreatScore.CreateNull());
    }

    /// <summary>
    /// Updates the verdict of the file report and recalculates the threat zone based on the new verdict.
    /// </summary>
    /// <param name="verdict">The new verdict to set for the file report.</param>
    /// <exception cref="ArgumentNullException">Thrown when the verdict is null.</exception>
    public void UpdateVerdict(Verdict verdict)
    {
        ArgumentNullException.ThrowIfNull(verdict);
        Verdict = verdict;
        ThreatZone = ThreatZoneMapping.Map[verdict];
    }

    /// <summary>
    /// Updates the threat score of the file report.
    /// </summary>
    /// <param name="threatScore">
    /// The new <see cref="ThreatScore"/> value to set for the file report.
    /// </param>
    public void UpdateThreatScore(ThreatScore threatScore)
    {
        ArgumentNullException.ThrowIfNull(threatScore);
        ThreatScore = threatScore;
    }

    /// <summary>
    /// Compares the current report with another report to determine if they have the same state.
    /// </summary>
    /// <param name="other">The other report to compare with.</param>
    /// <returns>
    /// True if the current report and the other report have the same verdict, threat zone,
    /// and threat score; otherwise, false.
    /// </returns>
    public bool HasSameStateTo(FileReport other)
    {
        return Verdict == other.Verdict
            && ThreatZone == other.ThreatZone
            && ThreatScore.Equals(other.ThreatScore);
    }
}
