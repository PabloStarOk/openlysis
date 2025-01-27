using Openlysis.Domain.Common.Models;
using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileReports.Entities;
using Openlysis.Domain.FileReports.Enums;
using Openlysis.Domain.FileReports.ValueObjects;
using File = Openlysis.Domain.FileReports.ValueObjects.File;

namespace Openlysis.Domain.FileReports;

/// <summary>
/// Represents a report for a file.
/// </summary>
public class FileAnalysis : AggregateRoot<FileAnalysisId>
{
    private readonly List<FileReport> _reports;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysis"/> class.
    /// </summary>
    /// <param name="id">ID of the file report.</param>
    /// <param name="lastScanDate">Last date when the file was scanned.</param>
    /// <param name="verdict">Verdict for the file.</param>
    /// <param name="file">File information.</param>
    /// <param name="reports">Analysis reports of the file.</param>
    private FileAnalysis(
        FileAnalysisId id,
        DateTime lastScanDate,
        Verdict verdict,
        File file,
        List<FileReport> reports)
        : base(id)
    {
        LastScanDate = lastScanDate;
        Verdict = verdict;
        File = file;
        _reports = reports;
    }

    /// <summary>
    /// Gets the last date when the file was scanned.
    /// </summary>
    public DateTime LastScanDate { get; private set; }

    /// <summary>
    /// Gets the amount of reports.
    /// </summary>
    public int ReportsAmount => _reports.Count;

    /// <summary>
    /// Gets the verdict of the analysis.
    /// </summary>
    public Verdict Verdict { get; private set; }

    /// <summary>
    /// Gets the information of the file.
    /// </summary>
    public File File { get; private set; }

    /// <summary>
    /// Gets the reports of the analysis.
    /// </summary>
    public IReadOnlyList<Report> Reports => _reports.ToList().AsReadOnly();

    /// <summary>
    /// Creates a new instance of a <see cref="FileAnalysis"/>.
    /// </summary>
    /// <param name="lastScanDate">Last date when the file was scanned.</param>
    /// <param name="verdict">Verdict for the file.</param>
    /// <param name="file">File information.</param>
    /// <param name="reports">Analysis reports of the file.</param>
    /// <returns>A <see cref="FileAnalysis"/>.</returns>
    /// <exception cref="ArgumentNullException">If reports list is null.</exception>
    public static FileAnalysis Create(
        DateTime lastScanDate,
        Verdict verdict,
        File file,
        List<FileReport> reports)
    {
        ArgumentNullException.ThrowIfNull(reports);

        return new FileAnalysis(
            FileAnalysisId.CreateUnique(),
            lastScanDate,
            verdict,
            file,
            reports);
    }

    /// <summary>
    /// Changes the last date when the file was scanned.
    /// </summary>
    /// <param name="newDate">New date of the last scan.</param>
    public void ChangeLastScanDate(DateTime newDate)
    {
        if (newDate < LastScanDate)
        {
            return; // TODO: Return error.
        }

        LastScanDate = newDate;
    }

    /// <summary>
    /// Adds a new report for the file.
    /// </summary>
    /// <param name="fileReport">New report to add.</param>
    public void AddReport(FileReport fileReport)
    {
        if (fileReport.Equals(null))
        {
            return; // TODO: Return error.
        }

        if (_reports.Contains(fileReport))
        {
            return; // TODO: Return error.
        }

        _reports.Add(fileReport);
    }
}
