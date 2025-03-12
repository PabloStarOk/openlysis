using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Models;
using Openlysis.Domain.Common.ServiceAnalyses.Mappings;
using Openlysis.Domain.Common.ServiceAnalyses.ValueObjects;

namespace Openlysis.Domain.URLs.Entities;

/// <summary>
/// Represents an analysis of a URL service.
/// </summary>
/// <remarks>
/// This class inherits from the Entity class with a <see cref="ServiceAnalysisId"/> type parameter.
/// </remarks>
public sealed class UrlServiceAnalysis : Entity<ServiceAnalysisId>
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the current status of the analysis.
    /// </summary>
    public AnalysisStatus Status { get; private set; }

    /// <summary>
    /// Gets the verdict of the analysis.
    /// </summary>
    public Verdict Verdict { get; private set; }

    /// <summary>
    /// Gets the threat zone associated with the analysis.
    /// </summary>
    public ThreatZone ThreatZone { get; private set; }

    /// <summary>
    /// Gets the threat score of the analysis.
    /// </summary>
    public float? ThreatScore { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlServiceAnalysis"/> class.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="threatScore">The threat score of the analysis.</param>
    public UrlServiceAnalysis(
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        float? threatScore)
    {
        ServiceName = serviceName;
        Status = status;
        UpdateVerdict(verdict);
        ThreatScore = threatScore;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private UrlServiceAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Updates the status of the analysis.
    /// </summary>
    /// <param name="newStatus">The new status to set.</param>
    public void UpdateStatus(AnalysisStatus newStatus)
    {
        Status = newStatus;
    }

    /// <summary>
    /// Updates the verdict of the analysis.
    /// </summary>
    /// <param name="newVerdict">The new verdict to set.</param>
    /// <remarks>
    /// The verdict can only be updated if the status is either Queued or InProgress.
    /// </remarks>
    public void UpdateVerdict(Verdict newVerdict)
    {
        if (Status is not AnalysisStatus.Queued and AnalysisStatus.InProgress)
        {
            return;
        }

        Verdict = newVerdict;
        ThreatZone = ThreatZoneMapping.Map[newVerdict];
    }
}