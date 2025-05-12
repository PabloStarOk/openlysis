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
    private readonly List<Report> _reports = [];

    /// <summary>
    /// Gets the reports associated with the analysis as a read-only dictionary.
    /// </summary>
    public IReadOnlyList<Report> Reports => _reports.AsReadOnly();

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
        List<Report> reports,
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
    /// <param name="reports">The list of reports associated with the analysis.</param>
    /// <param name="jobId">An optional job identifier associated with the analysis.</param>
    /// <returns>A new instance of <see cref="FileServiceAnalysis"/>.</returns>
    public static FileServiceAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        List<Report> reports,
        string? jobId = null)
    {
        var state = AnalysisState
            .Initial()
            .WithStatus(status);
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
    /// <param name="jobId">An optional job identifier associated with the analysis.</param>
    /// <returns>A new instance of <see cref="FileServiceAnalysis"/>.</returns>
    public static FileServiceAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        string? jobId = null)
    {
        var state = AnalysisState
            .Initial()
            .WithStatus(status);
        return new FileServiceAnalysis(
            ComposedServiceAnalysisId.Create(id, jobId),
            serviceName,
            state,
            [],
            error: null);
    }

    /// <summary>
    /// Creates a new instance of <see cref="FileServiceAnalysis"/> with failed status.
    /// </summary>
    /// <param name="serviceName">The name of the service that failed analysis.</param>
    /// <param name="error">The error message describing why the analysis failed.</param>
    /// <returns>A new instance of <see cref="FileServiceAnalysis"/> with failed status and empty identifiers.</returns>
    public static FileServiceAnalysis CreateFailed(
        string serviceName,
        string error)
    {
        string id = string.Empty;
        string jobId = string.Empty;
        return new FileServiceAnalysis(
            ComposedServiceAnalysisId.Create(id, jobId),
            serviceName,
            AnalysisState.CreateFailed(),
            [],
            error);
    }

    /// <summary>
    /// Adds a report to the analysis.
    /// </summary>
    /// <param name="report">The report to add.</param>
    public void AddReport(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (!State.CanBeUpdated)
        {
            throw new InvalidOperationException("Cannot add a report when analysis is completed, failed or timed out.");
        }

        if (_reports.Contains(report))
        {
            return;
        }

        _reports.Add(report);
        UpdateVerdictFromReports();
    }

    /// <summary>
    /// Updates an existing report in the analysis.
    /// </summary>
    /// <param name="report">The report to update.</param>
    public void UpdateReport(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (!State.CanBeUpdated)
        {
            throw new InvalidOperationException("Cannot add a report when analysis is completed, failed or timed out.");
        }

        if (!_reports.Contains(report))
        {
            return;
        }

        int reportIndex = _reports.IndexOf(report);
        _reports[reportIndex] = report;
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
        State = State.WithVerdict(best);
    }
}