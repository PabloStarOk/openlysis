using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Models;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Configuration;

namespace Openlysis.MultiAnalyzer.Infrastructure.Orchestrators;

/// <summary>
/// Orchestrates the multi-analysis process for URLs by coordinating multiple analyzers.
/// Inherits from <see cref="MultiAnalysisOrchestrator{TAnalysis, TAnalyzeMessage}"/>.
/// </summary>
internal sealed class UrlMultiAnalysisOrchestrator
    : MultiAnalysisOrchestrator<UrlAnalysis, AnalyzeUrlMessage>
{
    private readonly Dictionary<string, Analyzer<UrlAnalysis, AnalyzeUrlRequest>> _analyzers;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysisOrchestrator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging orchestration events.</param>
    /// <param name="options">The orchestration options.</param>
    /// <param name="updateMessageSender">The sender for update messages regarding multi-analysis.</param>
    /// <param name="analyzers">A collection of analyzers to be coordinated.</param>
    public UrlMultiAnalysisOrchestrator(
        ILogger<UrlMultiAnalysisOrchestrator> logger,
        IOptions<OrchestrationOptions> options,
        IUpdateMessageSender<UpdateMultiAnalysisMessage<UrlAnalysis>> updateMessageSender,
        IEnumerable<Analyzer<UrlAnalysis, AnalyzeUrlRequest>> analyzers)
        : base(logger, options, updateMessageSender)
    {
        _analyzers = analyzers.ToDictionary(a => a.ServiceName);
    }

    /// <inheritdoc/>
    protected override async Task<IEnumerable<UrlAnalysis>> HandleAnalyzeAsync(
        AnalyzeUrlMessage message,
        CancellationToken cancellationToken)
    {
        MultiAnalysisId = message.MultiAnalysisId;
        var analyzeRequest = new AnalyzeUrlRequest(message.Url);

        ConcurrentBag<UrlAnalysis> analyses = [];
        await Parallel.ForEachAsync(
            _analyzers.Values,
            cancellationToken,
            async (analyzer, _) =>
        {
            if (!analyzer.CanAnalyze)
            {
                Logger.LogWarning("Trying to analyze using an unavailable service.");
                return;
            }

            var analyzeResult = await analyzer.AnalyzeAsync(
                analyzeRequest,
                cancellationToken);
            if (analyzeResult.IsError)
            {
                Logger.LogError(
                    "One or more errors occurred while analyzing a file: {Errors}",
                    analyzeResult.Errors);
                return;
            }

            analyses.Add(analyzeResult.Value);
        });

        return analyses;
    }

    /// <inheritdoc/>
    protected override async ValueTask UpdateAnalysisAsync(
        UrlAnalysis pendingAnalysis,
        CancellationToken cancellationToken)
    {
        var analyzer = _analyzers[pendingAnalysis.ExternalId.Service];
        if (!analyzer.CanGetAnalysisStatus)
        {
            Logger.LogDebug("Trying to get analysis status when service is unavailable.");
            return;
        }

        // Get status
        ErrorOr<AnalysisStatus> getStatusResult =
            await analyzer.GetStatusAsync(pendingAnalysis.ExternalId, cancellationToken);
        if (getStatusResult.IsError)
        {
            pendingAnalysis.UpdateStatus(AnalysisStatus.Failed);
            PendingAnalyses.TryRemove(pendingAnalysis.Id, out _);
            UpdatableAnalyses.Add(pendingAnalysis);
            return;
        }

        // Update status
        if (getStatusResult.Value
            is AnalysisStatus.Queued
            or AnalysisStatus.InProgress)
        {
            return;
        }

        if (!analyzer.CanGetAnalysis)
        {
            Logger.LogDebug("Trying to get analysis when service is unavailable.");
            return;
        }

        // Get full analysis
        var identity = AnalysisIdentity.FromAnalysis(pendingAnalysis);
        ErrorOr<UrlAnalysis> getAnalysisResult = await analyzer.GetAnalysisAsync(
            identity,
            cancellationToken);
        if (getAnalysisResult.IsError)
        {
            return;
        }

        UrlAnalysis updatedAnalysis = getAnalysisResult.Value;
        UpdateAnalysisNewValues(pendingAnalysis, updatedAnalysis);
        PendingAnalyses.TryRemove(pendingAnalysis.Id, out _);
        UpdatableAnalyses.Add(pendingAnalysis);
    }

    /// <summary>
    /// Updates the values of the current analysis with new values from another analysis instance.
    /// </summary>
    /// <param name="currentAnalysis">The analysis to be updated.</param>
    /// <param name="newAnalysis">The analysis containing new values.</param>
    private static void UpdateAnalysisNewValues(
        UrlAnalysis currentAnalysis,
        UrlAnalysis newAnalysis)
    {
        currentAnalysis.UpdateVerdict(newAnalysis.State.Verdict);
        currentAnalysis.UpdateThreatScore(newAnalysis.ThreatScore);
        currentAnalysis.UpdateStatus(newAnalysis.State.Status);
    }

#if DEBUG
    /// <inheritdoc/>
    protected override void LogUpdateMessage(
        UpdateMultiAnalysisMessage<UrlAnalysis> message)
    {
        Logger.LogTrace(
            "Sending update multi analysis message:"
            + "\n\tID: {Id}"
            + "\n\tIs timeout: {Timeout}",
            message.MultiAnalysisId,
            message.Timeout);
    }

    /// <inheritdoc/>
    protected override void LogUpdatableAnalysis(UrlAnalysis analysis)
    {
        Logger.LogTrace(
            "Sending updatable analysis:"
            + "\n\tID: {Id}"
            + "\n\tExternal ID: {ExternalId}"
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreat score: {ThreatScore}"
            + "\n\tStatus: {Status}",
            analysis.Id,
            analysis.ExternalId,
            analysis.State.Verdict,
            analysis.ThreatScore,
            analysis.State.Status);
    }
#endif
}