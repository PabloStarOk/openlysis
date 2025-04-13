using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Files.ValueObjects;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Domain.Files;

/// <summary>
/// Represents a report for a file.
/// </summary>
public class FileMultiAnalysis : MultiAnalysis<FileServiceAnalysis>
{
    /// <summary>
    /// Gets the information of the file.
    /// </summary>
    public FileMetadata FileMetadata { get; init; }

    /// <summary>
    /// Gets all reports from the service file analyses.
    /// </summary>
    public IReadOnlyList<Report> AllReports => ServiceAnalyses
        .SelectMany(s => s.Reports).ToList().AsReadOnly();

    /// <summary>
    /// Gets the total number of reports.
    /// </summary>
    public int ReportsAmount => AllReports.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysis"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the file analysis.</param>
    /// <param name="userId">The unique identifier of the user who initiated the analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="status">The current status of the analysis.</param>
    /// <param name="averageVerdict">The summary verdict of the analysis.</param>
    /// <param name="averageThreatZone">The summary threat zone of the analysis.</param>
    /// <param name="dataHashSet">The set of content hashes associated with the analysis.</param>
    /// <param name="fileMetadata">The metadata of the file being analyzed.</param>
    private FileMultiAnalysis(
        GlobalId id,
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        AnalysisStatus status,
        Verdict averageVerdict,
        ThreatZone averageThreatZone,
        ContentHashSet dataHashSet,
        FileMetadata fileMetadata)
        : base(
            id,
            userId,
            isPrivate,
            startedDate,
            status,
            averageVerdict,
            averageThreatZone,
            dataHashSet)
    {
        FileMetadata = fileMetadata;
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
    /// Creates a new instance of the <see cref="FileMultiAnalysis"/> class.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who initiated the analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="startedDate">The date and time when the analysis started.</param>
    /// <param name="dataHashSet">The set of content hashes associated with the analysis.</param>
    /// <param name="fileMetadata">The metadata of the file being analyzed.</param>
    /// <returns>A new instance of the <see cref="FileMultiAnalysis"/> class.</returns>
    public static FileMultiAnalysis Create(
        UserId userId,
        bool isPrivate,
        DateTime startedDate,
        ContentHashSet dataHashSet,
        FileMetadata fileMetadata)
    {
        return new FileMultiAnalysis(
            GlobalId.CreateUnique(),
            userId,
            isPrivate,
            startedDate,
            AnalysisStatus.Queued,
            Verdict.Unknown,
            ThreatZone.Unknown,
            dataHashSet,
            fileMetadata);
    }

    /// <inheritdoc/>
    protected override void HandleServiceAnalysisUpdate(
        FileServiceAnalysis existingAnalysis,
        FileServiceAnalysis updatedAnalysis)
    {
        foreach (Report report in updatedAnalysis.Reports)
        {
            if (existingAnalysis.Reports.Contains(report))
            {
                existingAnalysis.UpdateReport(report);
                continue;
            }

            existingAnalysis.AddReport(report);
        }

        existingAnalysis.UpdateStatus(updatedAnalysis.Status);
    }

    /// <inheritdoc/>
    protected override void HandleAverageVerdictUpdate()
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

    /// <inheritdoc/>
    protected override void HandleAverageThreatScoreUpdate()
    {
        if (AllReports.All(r => r.ThreatScore is null))
        {
            return;
        }

        AverageThreatScore = AllReports
            .Where(r => r.ThreatScore is not null)
            .Select(r => r.ThreatScore)
            .Average();
    }
}
