using Openlysis.Analyzers.Filescan.Core.Models.Enums;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.Filescan.Core.Constants;



/// <summary>
/// Provides maps between Filescan service and Openlysis domain models.
/// </summary>
public static class Maps
{
    /// <summary>
    /// A dictionary that maps <see cref="Status"/> values to <see cref="AnalysisStatus"/> values.
    /// </summary>
    public static readonly IReadOnlyDictionary<Status, AnalysisStatus> AnalysisStatusMap = new Dictionary<Status, AnalysisStatus>()
    {
        { Status.Created, AnalysisStatus.Queued },
        { Status.Queued, AnalysisStatus.Queued },
        { Status.Scanning, AnalysisStatus.InProgress },
        { Status.Finished, AnalysisStatus.Completed },
    }.AsReadOnly();

    /// <summary>
    /// A read-only dictionary that maps <see cref="FilescanVerdict"/> values to <see cref="Verdict"/> values.
    /// </summary>
    public static readonly IReadOnlyDictionary<FilescanVerdict, Verdict> VerdictMap = new Dictionary<FilescanVerdict, Verdict>
    {
        { FilescanVerdict.Unknown, Verdict.Unknown },
        { FilescanVerdict.NoThreat, Verdict.Undetected },
        { FilescanVerdict.Benign, Verdict.Undetected },
        { FilescanVerdict.Suspicious, Verdict.Suspicious },
        { FilescanVerdict.LikelyMalicious, Verdict.Malicious },
        { FilescanVerdict.Malicious, Verdict.Malicious },
    };
}