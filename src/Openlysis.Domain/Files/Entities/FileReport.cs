using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Files.Entities;

/// <summary>
/// Report of an analysis for a file.
/// </summary>
public class FileReport : Entity<GlobalId>
{
    /// <summary>
    /// Gets the ID of the report given by an external analysis service.
    /// </summary>
    public string ExternalId { get;  }

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
    /// <param name="externalId">The ID given by an external service.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatScore">The threat score of the file.</param>
    private FileReport(
        GlobalId id,
        string externalId,
        Verdict verdict,
        ThreatZone threatZone,
        ThreatScore threatScore)
        : base(id)
    {
        ExternalId = externalId;
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
    /// <param name="externalId">The ID given by an external service.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatScore">The threat score of the file.</param>
    /// <returns>A new instance of the <see cref="FileReport"/> class with a normalized threat score.</returns>
    public static FileReport Create(
        string externalId,
        Verdict verdict,
        ThreatZone threatZone,
        ThreatScore? threatScore = null)
    {
        return new FileReport(
            GlobalId.CreateUnique(),
            externalId,
            verdict,
            threatZone,
            threatScore ?? ThreatScore.CreateNull());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FileReport"/> class with a specified ID.
    /// </summary>
    /// <param name="id">The unique identifier for the report.</param>
    /// <param name="externalId">The ID given by an external service.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatScore">The threat score of the file. If null, a default null threat score is used.</param>
    /// <returns>A new instance of the <see cref="FileReport"/> class.</returns>
    public static FileReport CreateWithId(
        GlobalId id,
        string externalId,
        Verdict verdict,
        ThreatZone threatZone,
        ThreatScore? threatScore = null)
    {
        ArgumentNullException.ThrowIfNull(id);

        return new FileReport(
            id,
            externalId,
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
}
