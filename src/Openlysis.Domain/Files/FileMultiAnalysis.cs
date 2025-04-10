using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Domain.Files;

/// <summary>
/// Represents a report for a file.
/// </summary>
public class FileMultiAnalysis : AggregateRoot<FileMultiAnalysisId>
{
    private readonly List<ServiceFileAnalysis> _serviceFileAnalyses = [];

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
    /// Gets the information of the file.
    /// </summary>
    public FileMetadata FileMetadata { get; init; }

    /// <summary>
    /// Gets the set of hash of the file.
    /// </summary>
    public ContentHashSet ContentHashSet { get; init; }

    /// <summary>
    /// Gets the list of service file analyses.
    /// </summary>
    public IReadOnlyList<ServiceFileAnalysis> ServiceFileAnalyses => _serviceFileAnalyses.AsReadOnly();

    /// <summary>
    /// Gets all reports from the service file analyses.
    /// </summary>
    public IReadOnlyList<Report> AllReports => _serviceFileAnalyses
        .SelectMany(s => s.Reports).ToList().AsReadOnly();

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
    /// <param name="fileMetadata">The metadata of the file.</param>
    /// <param name="contentHashSet">The set of hash of the file.</param>
    private FileMultiAnalysis(
        FileMultiAnalysisId id,
        List<ServiceFileAnalysis> serviceFileAnalyses,
        DateTime startedDate,
        Verdict averageVerdict,
        ThreatZone averageThreatZone,
        FileMetadata fileMetadata,
        ContentHashSet contentHashSet)
        : base(id)
    {
        _serviceFileAnalyses = serviceFileAnalyses;
        StartedDate = startedDate;
        AverageVerdict = averageVerdict;
        AverageThreatZone = averageThreatZone;
        FileMetadata = fileMetadata;
        ContentHashSet = contentHashSet;
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
    /// <param name="fileMetadata">The metadata of the file.</param>
    /// <param name="contentHashSet">The set of hash of the file.</param>
    /// <param name="serviceFileAnalyses">The dictionary of service file analyses.</param>
    /// <returns>A new instance of the <see cref="FileMultiAnalysis"/> class.</returns>
    public static FileMultiAnalysis Create(
        DateTime startedDate,
        FileMetadata fileMetadata,
        ContentHashSet contentHashSet,
        List<ServiceFileAnalysis> serviceFileAnalyses)
    {
        return new FileMultiAnalysis(
            FileMultiAnalysisId.CreateUnique(),
            serviceFileAnalyses,
            startedDate,
            Verdict.Unknown,
            ThreatZone.Unknown,
            fileMetadata,
            contentHashSet);
    }

    /// <summary>
    /// Adds a new service file analysis to the collection.
    /// </summary>
    /// <param name="analysis">The service file analysis to add.</param>
    public void AddServiceAnalysis(ServiceFileAnalysis analysis)
    {
        if (_serviceFileAnalyses.Contains(analysis))
        {
            return;
        }

        _serviceFileAnalyses.Add(analysis);
        UpdateAvgVerdict();
        UpdateAvgThreatZone();
        UpdateStatus();
    }

    /// <summary>
    /// Updates an existing service file analysis in the collection.
    /// </summary>
    /// <param name="analysis">The service file analysis to update.</param>
    public void UpdateServiceAnalysis(ServiceFileAnalysis analysis)
    {
        if (!_serviceFileAnalyses.Contains(analysis))
        {
            return;
        }

        int analysisIndex = _serviceFileAnalyses.IndexOf(analysis);
        _serviceFileAnalyses[analysisIndex] = analysis;
        UpdateAvgVerdict();
        UpdateAvgThreatZone();
        UpdateStatus();
    }

    /// <summary>
    /// Updates the status of the file analysis based on the statuses of all service file analyses.
    /// </summary>
    private void UpdateStatus()
    {
        IEnumerable<ServiceFileAnalysis> analyses = _serviceFileAnalyses;

        if (analyses.All(a => a.Status is AnalysisStatus.Completed))
        {
            Status = AnalysisStatus.Completed;
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
            AverageThreatZone = ThreatZone.Unknown;
            return;
        }

        var threatZoneCounts = AllReports.GroupBy(r => r.ThreatZone)
            .ToDictionary(g => g.Key, g => g.Count());

        AverageThreatZone = threatZoneCounts.OrderByDescending(pair => pair.Value)
            .ThenByDescending(pair => pair.Key)
            .First().Key;
    }
}
