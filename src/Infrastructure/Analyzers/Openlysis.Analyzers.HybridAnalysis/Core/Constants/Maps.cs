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
    /// A read-only dictionary that maps <see cref="HybridVerdict"/> values to <see cref="Verdict"/> values.
    /// </summary>
    public static readonly ReadOnlyDictionary<HybridVerdict, Verdict> VerdictMap = new Dictionary<HybridVerdict, Verdict>(Enum.GetValues<Status>().Length)
    {
        { HybridVerdict.Unknown, Verdict.Unknown },
        { HybridVerdict.NoVerdict, Verdict.Unknown },
        { HybridVerdict.NoSpecificThreat, Verdict.Undetected },
        { HybridVerdict.Suspicious, Verdict.Suspicious },
        { HybridVerdict.Malicious, Verdict.Malicious },
    }.AsReadOnly();
}
