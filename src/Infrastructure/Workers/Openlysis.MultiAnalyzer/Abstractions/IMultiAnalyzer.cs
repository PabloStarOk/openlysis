using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Openlysis.Analyzers.Shared.Contracts.Common.Models;
using Openlysis.Domain.Common.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.MultiAnalyzer.Abstractions;

/// <summary>
/// Defines a multi-analyzer that for <see cref="TMessage"/>.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis result.</typeparam>
/// <typeparam name="TMessage">The type of analysis job message.</typeparam>
internal interface IMultiAnalyzer<TAnalysis, in TMessage>
    where TAnalysis : Analysis
    where TMessage : AnalysisJobMessage
{
    /// <summary>
    /// Gets a channel reader for started analyses.
    /// </summary>
    public ChannelReader<TAnalysis> StartedAnalyses { get; }

    /// <summary>
    /// Analyzes the provided message, skipping analyses that have already started.
    /// </summary>
    /// <param name="message">The analysis job message to process.</param>
    /// <param name="alreadyStartedAnalyses">Array of identities for analyses that have already started.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AnalyzeAsync(
        TMessage message,
        AnalysisIdentity[] alreadyStartedAnalyses,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves analyses by their identities.
    /// </summary>
    /// <param name="analysisIdentities">Array of analysis identities to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of analysis results.</returns>
    public Task<IReadOnlyList<TAnalysis>> GetAnalysesByIdAsync(
        AnalysisIdentity[] analysisIdentities,
        CancellationToken cancellationToken = default);
}