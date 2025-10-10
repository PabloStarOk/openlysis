using System.Collections.ObjectModel;

using Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Constants;

/// <summary>
/// Provides mappings between <see cref="Status"/> and <see cref="AnalysisStatus"/>.
/// </summary>
public static class Maps
{
    /// <summary>
    /// A read-only dictionary that maps <see cref="Status"/> values to <see cref="AnalysisStatus"/> values.
    /// </summary>
    public static readonly ReadOnlyDictionary<Status, AnalysisStatus> AnalysisStatusMap = new Dictionary<Status, AnalysisStatus>(Enum.GetValues<Status>().Length)
    {
        { Status.InQueue, AnalysisStatus.Queued },
        { Status.InProgress, AnalysisStatus.InProgress },
        { Status.PartialSuccess, AnalysisStatus.InProgress },
        { Status.Success, AnalysisStatus.Completed },
        { Status.Error, AnalysisStatus.Failed },
    }.AsReadOnly();

    /// <summary>
    /// A read-only dictionary that maps <see cref="HybridAnalysisVerdict"/> values to <see cref="Verdict"/> values.
    /// </summary>
    public static readonly ReadOnlyDictionary<HybridAnalysisVerdict, Verdict> VerdictMap = new Dictionary<HybridAnalysisVerdict, Verdict>(Enum.GetValues<Status>().Length)
    {
        { HybridAnalysisVerdict.Unknown, Verdict.Unknown },
        { HybridAnalysisVerdict.NoVerdict, Verdict.Unknown },
        { HybridAnalysisVerdict.NoSpecificThreat, Verdict.Undetected },
        { HybridAnalysisVerdict.Suspicious, Verdict.Suspicious },
        { HybridAnalysisVerdict.Malicious, Verdict.Malicious },
    }.AsReadOnly();
}
