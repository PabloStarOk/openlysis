using Openlysis.Domain.Common.Constants;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Files.Entities;

/// <summary>
/// Represents a file analysis of a service.
/// </summary>
public class FileServiceAnalysis : ServiceAnalysis
{
    private readonly List<FileReport> _reports = [];

    /// <summary>
    /// Gets the reports associated with the analysis as a read-only dictionary.
    /// </summary>
    public IReadOnlyList<FileReport> Reports => _reports.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileServiceAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="state">The current state of the analysis.</param>
    /// <param name="reports">The dictionary of reports associated with the analysis.</param>
    /// <param name="error">The error message if the analysis failed.</param>
    private FileServiceAnalysis(
        ComposedServiceAnalysisId id,
        string serviceName,
        AnalysisState state,
        List<FileReport> reports,
        string? error)
        : base(id, serviceName, state, error)
    {
        _reports = reports;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private FileServiceAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="FileServiceAnalysis"/> with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="reports">The list of reports associated with the analysis.</param>
    /// <param name="jobId">An optional job identifier associated with the analysis.</param>
    /// <returns>A new instance of <see cref="FileServiceAnalysis"/>.</returns>
    public static FileServiceAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        List<FileReport> reports,
        string? jobId = null)
    {
        var state = AnalysisState.Initial().WithVerdict(verdict).WithStatus(status);
        return new FileServiceAnalysis(
            ComposedServiceAnalysisId.Create(id, jobId),
            serviceName,
            state,
            reports,
            error: null);
    }

    /// <summary>
    /// Creates a new instance of <see cref="FileServiceAnalysis"/> with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <param name="verdict">The verdict of the analysis.</param>
    /// <param name="jobId">An optional job identifier associated with the analysis.</param>
    /// <returns>A new instance of <see cref="FileServiceAnalysis"/>.</returns>
    public static FileServiceAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        Verdict verdict,
        string? jobId = null)
    {
        var state = AnalysisState.Initial().WithVerdict(verdict).WithStatus(status);
        return new FileServiceAnalysis(
            ComposedServiceAnalysisId.Create(id, jobId),
            serviceName,
            state,
            [],
            error: null);
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
    /// Determines whether the current analysis has the same state as the specified analysis.
    /// </summary>
    /// <param name="other">The other <see cref="FileServiceAnalysis"/> to compare with.</param>
    /// <returns>
    /// <c>true</c> if the current analysis has the same state as the specified analysis; otherwise, <c>false</c>.
    /// </returns>
    public bool HasSameStateTo(FileServiceAnalysis other)
    {
        return Reports.SequenceEqual(other.Reports)
            && State == other.State;
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