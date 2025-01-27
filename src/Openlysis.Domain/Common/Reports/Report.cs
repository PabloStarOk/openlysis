using Openlysis.Domain.Common.Models;

namespace Openlysis.Domain.Common.Reports;

/// <summary>
/// Represents a single report.
/// </summary>
public abstract class Report : Entity<ReportId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Report"/> class.
    /// </summary>
    /// <param name="id">ID of the report.</param>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="scanState">State of the scan.</param>
    /// <param name="scanStartDate">Start date of the scan.</param>
    /// <param name="scanEndDate">End date of the scan.</param>
    /// <param name="verdict">Verdict of the scan.</param>
    protected Report(
        ReportId id,
        string serviceName,
        ScanState scanState,
        DateTime scanStartDate,
        ScanState scanEndDate,
        string verdict)
        : base(id)
    {
        ServiceName = serviceName;
        ScanState = scanState;
        ScanStartDate = scanStartDate;
        ScanEndDate = scanEndDate;
        Verdict = verdict;
    }

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string ServiceName { get; protected set; }

    /// <summary>
    /// Gets or sets the state of the scan.
    /// </summary>
    public ScanState ScanState { get; protected set; }

    /// <summary>
    /// Gets or sets the start date of the scan.
    /// </summary>
    public DateTime ScanStartDate { get; protected set; }

    /// <summary>
    /// Gets or sets the end date of the scan.
    /// </summary>
    public ScanState ScanEndDate { get; protected set; }

    /// <summary>
    /// Gets or sets the verdict of the scan.
    /// </summary>
    public string Verdict { get; protected set; }
}
