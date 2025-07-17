using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Files.Entities;

/// <summary>
/// An analysis for a file performed by an external service.
/// </summary>
public sealed class FileAnalysis : Analysis
{
    private readonly List<FileReport> _reports = [];

    /// <summary>
    /// Gets the reports associated with the analysis.
    /// </summary>
    public IReadOnlyList<FileReport> Reports => _reports.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="externalId">The identifiers assigned by the external analysis service.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="reports">The dictionary of reports associated with the analysis.</param>
    /// <param name="threatScore">The threat score assigned by the service.</param>
    private FileAnalysis(
        GlobalId id,
        ExternalAnalysisId externalId,
        string serviceName,
        AnalysisState state,
        List<FileReport> reports,
        ThreatScore threatScore)
        : base(id, externalId, serviceName, state, threatScore)
    {
        _reports = reports;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private FileAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="FileAnalysis"/> with the specified parameters,
    /// but without any initial file reports.
    /// </summary>
    /// <param name="externalPrimaryId">The primary identifier assigned by the external service.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="externalJobId">An optional job identifier assigned by the external service.</param>
    /// <param name="threatScore">The threat score assigned by the service.</param>
    /// <returns>A new instance of <see cref="FileAnalysis"/> with no file reports.</returns>
    public static FileAnalysis Create(
        string externalPrimaryId,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        string? externalJobId = null,
        ThreatScore? threatScore = null)
    {
        var state = AnalysisState.Initial().WithVerdict(verdict).WithStatus(status);
        return new FileAnalysis(
            GlobalId.CreateUnique(),
            ExternalAnalysisId.Create(externalPrimaryId, externalJobId),
            serviceName,
            state,
            [],
            threatScore ?? ThreatScore.CreateNull());
    }

    /// <summary>
    /// Creates a new instance of <see cref="FileAnalysis"/> with a specified unique identifier and external analysis IDs.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="externalId">The identifiers assigned by the external analysis service.</param>
    /// <param name="serviceName">The name of the service that performed the analysis.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="reports">The list of reports associated with the analysis.</param>
    /// <param name="threatScore">The threat score assigned by the service (optional).</param>
    /// <returns>A new instance of <see cref="FileAnalysis"/>.</returns>
    public static FileAnalysis CreateWithId(
        GlobalId id,
        ExternalAnalysisId externalId,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        List<FileReport> reports,
        ThreatScore? threatScore = null)
    {
        var state = AnalysisState.Initial().WithVerdict(verdict).WithStatus(status);
        return new FileAnalysis(
            id,
            externalId,
            serviceName,
            state,
            reports,
            threatScore ?? ThreatScore.CreateNull());
    }

    /// <summary>
    /// Adds a new file report to the analysis.
    /// </summary>
    /// <param name="fileReport">The file report to add to the analysis.</param>
    /// <exception cref="ArgumentNullException">Thrown when the file report is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the analysis state doesn't allow updates or when the report already exists.</exception>
    public void AddReport(FileReport fileReport)
    {
        ArgumentNullException.ThrowIfNull(fileReport);

        if (!State.CanBeUpdated)
        {
            throw new InvalidOperationException("Cannot add a fileReport when analysis is completed, failed or timed out.");
        }

        if (_reports.Contains(fileReport))
        {
            throw new InvalidOperationException("File report already exists.");
        }

        _reports.Add(fileReport);
        UpdateVerdictFromReports();
    }

    /// <summary>
    /// Updates an existing file report with new verdict and threat score information.
    /// </summary>
    /// <param name="updatedReport">The file report containing updated information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="updatedReport"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when analysis state doesn't allow updates.</exception>
    public void UpdateReport(FileReport updatedReport)
    {
        ArgumentNullException.ThrowIfNull(updatedReport);

        if (!State.CanBeUpdated)
        {
            throw new InvalidOperationException("Cannot update a file report when analysis is completed, failed or timed out.");
        }

        FileReport existingReport = _reports.Single(r => r.Id == updatedReport.Id);
        existingReport.UpdateVerdict(updatedReport.Verdict);
        existingReport.UpdateThreatScore(updatedReport.ThreatScore);
        UpdateVerdictFromReports();
    }

    /// <summary>
    /// Updates the verdict of the analysis based on the highest verdict among the associated reports.
    /// </summary>
    private void UpdateVerdictFromReports()
    {
        var best = _reports
            .Select(r => r.Verdict)
            .OrderByDescending(v => VerdictRankMapping.Map[v])
            .FirstOrDefault();
        UpdateVerdict(best);
    }
}