using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Files.ValueObjects;

namespace Openlysis.Domain.Files;

/// <summary>
/// An aggregate that contains multiple analyses for a file.
/// </summary>
public class FileMultiAnalysis : MultiAnalysis<FileAnalysis>
{
    /// <summary>
    /// Gets the information of the file.
    /// </summary>
    public FileMetadata FileMetadata { get; init; }

    /// <summary>
    /// Gets all reports from the file analyses.
    /// </summary>
    public IReadOnlyList<FileReport> AllReports => Analyses
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
    /// <param name="state">The initial state of the analysis, including its status, verdict, and threat zone.</param>
    /// <param name="dataHashValues">The set of content hashes associated with the analysis.</param>
    /// <param name="fileMetadata">The metadata of the file being analyzed.</param>
    private FileMultiAnalysis(
        GlobalId id,
        GlobalId userId,
        bool isPrivate,
        DateTime startedDate,
        AnalysisState state,
        HashValues dataHashValues,
        FileMetadata fileMetadata)
        : base(
            id,
            userId,
            isPrivate,
            startedDate,
            state,
            dataHashValues)
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
    /// <param name="dataHashValues">The set of content hashes associated with the analysis.</param>
    /// <param name="fileMetadata">The metadata of the file being analyzed.</param>
    /// <returns>A new instance of the <see cref="FileMultiAnalysis"/> class.</returns>
    public static FileMultiAnalysis Create(
        GlobalId userId,
        bool isPrivate,
        DateTime startedDate,
        HashValues dataHashValues,
        FileMetadata fileMetadata)
    {
        return new FileMultiAnalysis(
            GlobalId.CreateUnique(),
            userId,
            isPrivate,
            startedDate,
            AnalysisState.Initial(),
            dataHashValues,
            fileMetadata);
    }

    /// <inheritdoc/>
    protected override void HandleAnalysisUpdate(
        FileAnalysis existingAnalysis,
        FileAnalysis updatedAnalysis)
    {
        foreach (FileReport report in updatedAnalysis.Reports)
        {
            if (existingAnalysis.Reports.Contains(report))
            {
                existingAnalysis.UpdateReport(report);
                continue;
            }

            existingAnalysis.AddReport(report);
        }

        existingAnalysis.UpdateVerdict(updatedAnalysis.State.Verdict);
        existingAnalysis.UpdateThreatScore(updatedAnalysis.ThreatScore);
        existingAnalysis.UpdateStatus(updatedAnalysis.State.Status);
    }

    /// <inheritdoc/>
    protected override void HandleAverageThreatScoreUpdate()
    {
        int?[] allScores = [
            ..Analyses
                .SelectMany(s => s.Reports)
                .Select(r => r.ThreatScore.NormalizedValue)
                .Where(t => t is not null),
            ..Analyses
                .Select(s => s.ThreatScore.NormalizedValue)
                .Where(t => t is not null),
        ];

        if (allScores.Length is 0)
        {
            return;
        }

        double? average = allScores.Average();
        if (!average.HasValue)
        {
            return;
        }

        AverageThreatScore = (int?)Math.Round(average.Value);
    }
}
