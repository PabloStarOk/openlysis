using System.Collections.Generic;

using Openlysis.Analyzers.Shared.Contracts.Common.Models;

namespace Openlysis.AnalysisOrchestrator.Communication.Consumers.Common;

/// <summary>
/// Represents the state of an analysis job consumer, including its status and the set of started analysis identities.
/// </summary>
internal sealed class AnalysisJobConsumerState
{
    /// <summary>
    /// Gets or sets the current status of the analysis job consumer.
    /// </summary>
    public AnalysisJobConsumerStatus Status { get; set; }

    /// <summary>
    /// Gets the set of analysis identities that have been started.
    /// </summary>
    public HashSet<AnalysisIdentity> StartedAnalysisIdentities { get; init; } = [];
}