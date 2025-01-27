using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileReports.ValueObjects;

namespace Openlysis.Domain.FileReports.Entities;

/// <summary>
/// Represents a report of a file.
/// </summary>
public class FileReport : Report
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileReport"/> class.
    /// </summary>
    /// <param name="id">ID of the report.</param>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="scanState">State of the scan.</param>
    /// <param name="scanStartDate">Start date of the scan.</param>
    /// <param name="scanEndDate">End date of the scan.</param>
    /// <param name="verdict">Verdict of the scan.</param>
    /// <param name="detectionInfo">Detection information details.</param>
    public FileReport(
        ReportId id,
        string serviceName,
        ScanState scanState,
        DateTime scanStartDate,
        ScanState scanEndDate,
        string verdict,
        DetectionInfo detectionInfo)
        : base(
            id,
            serviceName,
            scanState,
            scanStartDate,
            scanEndDate,
            verdict)
    {
        DetectionInfo = detectionInfo;
    }

    /// <summary>
    /// Gets or sets the detection information details of the scan.
    /// </summary>
    public DetectionInfo DetectionInfo { get; protected set; }
}