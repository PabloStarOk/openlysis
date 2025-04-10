using Openlysis.Analyzers.VirusTotal.Core.Models.Enums;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.VirusTotal.Core.Constants;

/// <summary>
/// Provides mappings between <see cref="Status"/> and <see cref="AnalysisStatus"/>.
/// </summary>
public static class Maps
{
    /// <summary>
    /// A read-only dictionary that maps <see cref="Status"/> values to <see cref="AnalysisStatus"/> values.
    /// </summary>
    public static readonly IReadOnlyDictionary<Status, AnalysisStatus> AnalysisStatusMap = new Dictionary<Status, AnalysisStatus>
    {
        { Status.Queued, AnalysisStatus.Queued },
        { Status.InProgress, AnalysisStatus.InProgress },
        { Status.Completed, AnalysisStatus.Completed },
    }.AsReadOnly();
}