using Openlysis.Analyzers.URLQuery.Core.Models.Enums;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Constants;

/// <summary>
/// Provides mappings between <see cref="Status"/> and <see cref="AnalysisStatus"/>.
/// </summary>
internal static class Maps
{
    /// <summary>
    /// A read-only dictionary that maps <see cref="Status"/> values to <see cref="AnalysisStatus"/> values.
    /// </summary>
    internal static readonly IReadOnlyDictionary<Status, AnalysisStatus> AnalysisStatusMap = new Dictionary<Status, AnalysisStatus>()
    {
        { Status.Queued, AnalysisStatus.Queued },
        { Status.Processing, AnalysisStatus.InProgress },
        { Status.Analyzing, AnalysisStatus.InProgress },
        { Status.Done, AnalysisStatus.Completed },
        { Status.Failed, AnalysisStatus.Failed },
    };

    /// <summary>
    /// A read-only dictionary that maps <see cref="Status"/> values to <see cref="AnalysisStatus"/> values.
    /// </summary>
    internal static readonly IReadOnlyDictionary<UserAgentType, string> UserAgentsMap = new Dictionary<UserAgentType, string>()
    {
        { UserAgentType.Mobile, UserAgents.Mobile },
        { UserAgentType.Linux, UserAgents.Linux },
        { UserAgentType.Windows, UserAgents.Windows },
    };
}