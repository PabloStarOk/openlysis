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
    public float? ThreatScore { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Report"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the report.</param>
    /// <param name="verdict">The verdict of the scan.</param>
    /// <param name="threatZone">The threat zone of the scan.</param>
    /// <param name="threatScore">The threat level of the scan. Optional.</param>
    protected Report(
        ReportId id,
        Verdict verdict,
        ThreatZone threatZone,
        float? threatScore = null)
        : base(id)
    {
        Verdict = verdict;
        ThreatZone = threatZone;
        ThreatScore = threatScore;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private Report()
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
    /// <param name="threatScore">The threat score of the scan.</param>
    /// <returns>A new instance of the <see cref="Report"/> class.</returns>
    public static Report Create(
        string id,
        Verdict verdict,
        ThreatZone threatZone,
        float? threatScore)
    {
        float? normalizedThreatScore = NormalizeThreatScore(threatScore);
        return new Report(
            ReportId.Create(id),
            verdict,
            threatZone,
            normalizedThreatScore);
    }

    /// <summary>
    /// Compares the current report with another report to determine if they have the same state.
    /// </summary>
    /// <param name="other">The other report to compare with.</param>
    /// <returns>
    /// True if the current report and the other report have the same verdict, threat zone,
    /// and threat score; otherwise, false.
    /// </returns>
    public bool HasSameStateTo(Report other)
    {
        return Verdict == other.Verdict
            && ThreatZone == other.ThreatZone
            && ThreatScore.Equals(other.ThreatScore);
    }

    /// <summary>
    /// Normalizes the given threat score to ensure it falls within the range of 0.0 to 1.0.
    /// </summary>
    /// <param name="threatScore">The threat score to normalize. Can be null.</param>
    /// <returns>
    /// A normalized threat score between 0.0 and 1.0, or null if the input is null.
    /// If the input is greater than 1.0, it is divided by 100.0 before clamping.
    /// </returns>
    private static float? NormalizeThreatScore(float? threatScore)
    {
        switch (threatScore)
        {
            case null:
                return null;
            case > 1.0f:
                threatScore /= 100.0f;
                break;
        }

        return Math.Clamp((float)threatScore, 0.0f, 1.0f);
    }
}
