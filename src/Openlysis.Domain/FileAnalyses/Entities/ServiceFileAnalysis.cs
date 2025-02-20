using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Models;
using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Domain.FileAnalyses.Entities;

/// <summary>
/// Represents a file analysis of a service.
/// </summary>
public class ServiceFileAnalysis : Entity<ServiceFileAnalysisId>
{
    private readonly Dictionary<ReportId, Report> _reports;

    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the status of the analysis.
    /// </summary>
    public AnalysisStatus Status { get; private set; }

    /// <summary>
    /// Gets the reports associated with the analysis as a read-only dictionary.
    /// </summary>
    public IReadOnlyList<Report> Reports => _reports.Values.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceFileAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <param name="reports">The dictionary of reports associated with the analysis.</param>
    private ServiceFileAnalysis(
        ServiceFileAnalysisId id,
        string serviceName,
        AnalysisStatus status,
        Dictionary<ReportId, Report> reports)
        : base(id)
    {
        ServiceName = serviceName;
        Status = status;
        _reports = reports;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private ServiceFileAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of <see cref="ServiceFileAnalysis"/> with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <param name="reports">The dictionary of reports associated with the analysis.</param>
    /// <returns>A new instance of <see cref="ServiceFileAnalysis"/>.</returns>
    public static ServiceFileAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        Dictionary<ReportId, Report> reports)
    {
        return new ServiceFileAnalysis(
            ServiceFileAnalysisId.Create(id),
            serviceName,
            status,
            reports);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ServiceFileAnalysis"/> with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <returns>A new instance of <see cref="ServiceFileAnalysis"/>.</returns>
    public static ServiceFileAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status)
    {
        return new ServiceFileAnalysis(
            ServiceFileAnalysisId.Create(id),
            serviceName,
            status,
            []);
    }

    /// <summary>
    /// Updates the status of the analysis.
    /// </summary>
    /// <param name="status">The new status to set.</param>
    public void UpdateStatus(AnalysisStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        Status = status;
    }

    /// <summary>
    /// Adds a report to the analysis.
    /// </summary>
    /// <param name="report">The report to add.</param>
    public void AddReport(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        _reports.TryAdd(report.Id, report);
    }

    /// <summary>
    /// Updates an existing report in the analysis.
    /// </summary>
    /// <param name="report">The report to update.</param>
    public void UpdateReport(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (!_reports.ContainsKey(report.Id))
        {
            return;
        }

        _reports[report.Id] = report;
    }
}