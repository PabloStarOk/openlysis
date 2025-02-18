using FluentResults;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.Models;
using Openlysis.Domain.Common.Reports;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Domain.FileAnalyses;

/// <summary>
/// Represents a report for a file.
/// </summary>
public class FileMultiAnalysis : AggregateRoot<FileMultiAnalysisId>
{
    private readonly Dictionary<ServiceFileAnalysisId, ServiceFileAnalysis> _serviceFileAnalyses;

    /// <summary>
    /// Gets the date and time when the analysis started.
    /// </summary>
    public DateTime StartedDate { get; }

    /// <summary>
    /// Gets the verdict of the analysis.
    /// </summary>
    public Verdict AverageVerdict { get; private set; }

    /// <summary>
    /// Gets the summary threat zone of the analysis.
    /// </summary>
    public ThreatZone AverageThreatZone { get; private set; }

    /// <summary>
    /// Gets the status of the analysis.
    /// </summary>
    public AnalysisStatus Status { get; private set; }

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the information of the file.
    /// </summary>
    public FileMetadata Metadata { get; }

    /// <summary>
    /// Gets the list of service file analyses.
    /// </summary>
    public IReadOnlyDictionary<ServiceFileAnalysisId, ServiceFileAnalysis> ServiceFileAnalyses => _serviceFileAnalyses;

    /// <summary>
    /// Gets all reports from the service file analyses.
    /// </summary>
    public IReadOnlyList<Report> AllReports { get; private set; } = [];

    /// <summary>
    /// Gets the total number of reports.
    /// </summary>
    public int ReportsAmount => AllReports.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the file analysis.</param>
    /// <param name="serviceFileAnalyses">The list of service file analyses.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="averageVerdict">The summary verdict of the analysis.</param>
    /// <param name="averageThreatZone">The summary threat zone of the analysis.</param>
    /// <param name="fileName">The name of the file being analyzed.</param>
    /// <param name="metadata">The metadata information of the file.</param>
    private FileMultiAnalysis(
        FileMultiAnalysisId id,
        Dictionary<ServiceFileAnalysisId, ServiceFileAnalysis> serviceFileAnalyses,
        DateTime startedDate,
        Verdict averageVerdict,
        ThreatZone averageThreatZone,
        string fileName,
        FileMetadata metadata)
        : base(id)
    {
        _serviceFileAnalyses = serviceFileAnalyses;
        StartedDate = startedDate;
        AverageVerdict = averageVerdict;
        AverageThreatZone = averageThreatZone;
        FileName = fileName;
        Metadata = metadata;
    }

    // For EF core.
#pragma warning disable CS8618
#pragma warning disable S1144
    private FileMultiAnalysis()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="FileMultiAnalysis"/> class with the specified file name, metadata, and reports.
    /// </summary>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="fileName">The name of the file being analyzed.</param>
    /// <param name="metadata">The metadata information of the file.</param>
    /// <param name="serviceFileAnalyses">The list of service file analyses.</param>
    /// <returns>A new instance of the <see cref="FileMultiAnalysis"/> class.</returns>
    public static FileMultiAnalysis Create(
        DateTime startedDate,
        string fileName,
        FileMetadata metadata,
        Dictionary<ServiceFileAnalysisId, ServiceFileAnalysis> serviceFileAnalyses)
    {
        return new FileMultiAnalysis(
            FileMultiAnalysisId.CreateUnique(),
            serviceFileAnalyses,
            startedDate,
            Verdict.Unknown,
            ThreatZone.None,
            fileName,
            metadata);
    }

    /// <summary>
    /// Adds a new service file analysis to the collection.
    /// </summary>
    /// <param name="analysis">The service file analysis to add.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Result AddServiceAnalysis(ServiceFileAnalysis analysis)
    {
        if (!_serviceFileAnalyses.TryAdd(analysis.Id, analysis))
        {
            return Result.Fail("Already exists an analysis with the same ID.");
        }

        AllReports = _serviceFileAnalyses.Values.SelectMany(s => s.Reports.Values).ToList().AsReadOnly();
        UpdateAvgVerdict();
        UpdateAvgThreatZone();
        UpdateStatus();

        return Result.Ok();
    }

    /// <summary>
    /// Updates an existing service file analysis in the collection.
    /// </summary>
    /// <param name="analysis">The service file analysis to update.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Result UpdateServiceAnalysis(ServiceFileAnalysis analysis)
    {
        if (!_serviceFileAnalyses.ContainsKey(analysis.Id))
        {
            return Result.Fail("Analysis doesn't exist.");
        }

        AllReports = _serviceFileAnalyses.Values.SelectMany(s => s.Reports.Values).ToList().AsReadOnly();
        UpdateAvgVerdict();
        UpdateAvgThreatZone();
        UpdateStatus();

        _serviceFileAnalyses[analysis.Id] = analysis;
        return Result.Ok();
    }

    /// <summary>
    /// Updates the status of the file analysis based on the statuses of all service file analyses.
    /// </summary>
    private void UpdateStatus()
    {
        IEnumerable<ServiceFileAnalysis> analyses = _serviceFileAnalyses.Values;

        if (analyses.All(a => a.Status is AnalysisStatus.Finished))
        {
            Status = AnalysisStatus.Finished;
            return;
        }

        if (analyses.Any(a => a.Status is AnalysisStatus.Timeout))
        {
            Status = AnalysisStatus.Timeout;
            return;
        }

        var statusCount = analyses.GroupBy(a => a.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        Status = statusCount.OrderBy(s => s.Value)
            .ThenBy(s => s.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the average verdict based on all reports.
    /// </summary>
    private void UpdateAvgVerdict()
    {
        if (!AllReports.Any())
        {
            AverageVerdict = Verdict.Unknown;
            return;
        }

        var verdictCounts = AllReports.GroupBy(r => r.Verdict)
            .ToDictionary(g => g.Key, g => g.Count());

        AverageVerdict = verdictCounts
            .OrderByDescending(pair => pair.Value)
            .ThenByDescending(pair => pair.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the average threat zone based on all reports.
    /// </summary>
    private void UpdateAvgThreatZone()
    {
        if (!AllReports.Any())
        {
            AverageThreatZone = ThreatZone.None;
            return;
        }

        var threatZoneCounts = AllReports.GroupBy(r => r.ThreatZone)
            .ToDictionary(g => g.Key, g => g.Count());

        AverageThreatZone = threatZoneCounts.OrderByDescending(pair => pair.Value)
            .ThenByDescending(pair => pair.Key)
            .First().Key;
    }
}
