using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Files.Entities;

/// <summary>
/// Represents a file analysis of a service.
/// </summary>
public class ServiceFileAnalysis : ServiceAnalysis
{
    private readonly List<Report> _reports = [];

    /// <summary>
    /// Gets the reports associated with the analysis as a read-only dictionary.
    /// </summary>
    public IReadOnlyList<Report> Reports => _reports.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceFileAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the analysis.</param>
    /// <param name="serviceName">The name of the service being analyzed.</param>
    /// <param name="status">The initial status of the analysis.</param>
    /// <param name="reports">The dictionary of reports associated with the analysis.</param>
    private ServiceFileAnalysis(
        ComposedServiceAnalysisId id,
        string serviceName,
        AnalysisStatus status,
        List<Report> reports)
        : base(id, serviceName, status)
    {
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
    /// <param name="reports">The list of reports associated with the analysis.</param>
    /// <param name="jobId">An optional job identifier associated with the analysis.</param>
    /// <returns>A new instance of <see cref="ServiceFileAnalysis"/>.</returns>
    public static ServiceFileAnalysis Create(
        string id,
        string serviceName,
        AnalysisStatus status,
        List<Report> reports,
        string? jobId = "")
    {
        return new ServiceFileAnalysis(
            ComposedServiceAnalysisId.Create(id, jobId),
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
            ComposedServiceAnalysisId.Create(id),
            serviceName,
            status,
            []);
    }

    /// <summary>
    /// Adds a report to the analysis.
    /// </summary>
    /// <param name="report">The report to add.</param>
    public void AddReport(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (_reports.Contains(report))
        {
            return;
        }

        _reports.Add(report);
    }

    /// <summary>
    /// Updates an existing report in the analysis.
    /// </summary>
    /// <param name="report">The report to update.</param>
    public void UpdateReport(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (!_reports.Contains(report))
        {
            return;
        }

        int reportIndex = _reports.IndexOf(report);
        _reports[reportIndex] = report;
    }
}