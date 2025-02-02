using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileReports.ValueObjects;

namespace Openlysis.Domain.FileReports.Entities;

/// <summary>
/// Represents a report of a file.
/// </summary>
public sealed class FileReport : Report, IEquatable<FileReport>
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
    /// Gets the detection information details of the scan.
    /// </summary>
    public DetectionInfo DetectionInfo { get; private set; }

    /// <inheritdoc/>
    public bool Equals(FileReport? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((FileReport)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}