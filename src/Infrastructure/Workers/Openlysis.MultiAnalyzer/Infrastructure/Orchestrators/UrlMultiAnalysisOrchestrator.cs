using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
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
    : MultiAnalysisOrchestrator<UrlAnalysis, AnalyzeUrl>
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
        IUpdateMessageSender<UpdateMultiAnalysis<UrlAnalysis>> updateMessageSender,
        IEnumerable<Analyzer<UrlAnalysis, AnalyzeUrlRequest>> analyzers)
        : base(logger, options, updateMessageSender)
    {
        _analyzers = analyzers.ToDictionary(a => a.ServiceName);
    }

    /// <inheritdoc/>
    protected override async Task<IEnumerable<UrlAnalysis>> HandleAnalyzeAsync(
        AnalyzeUrl message,
        CancellationToken cancellationToken)
    {
        MultiAnalysisId = message.MultiAnalysisId;
        var analyzeRequest = new AnalyzeUrlRequest(message.Url);

        ConcurrentBag<UrlAnalysis> analyses = [];
        await Parallel.ForEachAsync(
            _analyzers.Values,
            cancellationToken,
            async (analyzer, ct) =>
        {
            if (!analyzer.CanAnalyze)
            {
                Logger.LogWarning("Trying to analyze using an unavailable service.");
                return;
            }

            var analyzeResult = await analyzer.AnalyzeAsync(analyzeRequest, ct);
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
        UrlAnalysis analysis,
        CancellationToken cancellationToken)
    {
        var analyzer = _analyzers[analysis.ServiceName];
        if (!analyzer.CanGetAnalysisStatus)
        {
            Logger.LogDebug("Trying to get analysis status when service is unavailable.");
            return;
        }

        // Get status
        ErrorOr<AnalysisStatus> getStatusResult =
            await analyzer.GetStatusAsync(analysis.Id, cancellationToken);
        if (getStatusResult.IsError)
        {
            analysis.UpdateStatus(AnalysisStatus.Failed);
            PendingAnalyses.TryRemove(analysis.Id, out _);
            UpdatableAnalyses.Add(analysis);
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
        ErrorOr<UrlAnalysis> getAnalysisResult
            = await analyzer.GetAnalysisAsync(analysis.Id, cancellationToken);
        if (getAnalysisResult.IsError)
        {
            return;
        }

        UrlAnalysis updatedAnalysis = getAnalysisResult.Value;
        PendingAnalyses.TryRemove(updatedAnalysis.Id, out _);
        UpdatableAnalyses.Add(updatedAnalysis);
    }

#if DEBUG
    /// <inheritdoc/>
    protected override void LogUpdateMessage(
        UpdateMultiAnalysis<UrlAnalysis> message)
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
            + "\n\tVerdict: {Verdict}"
            + "\n\tThreat score: {ThreatScore}"
            + "\n\tStatus: {Status}",
            analysis.Id,
            analysis.State.Verdict,
            analysis.ThreatScore,
            analysis.State.Status);
    }
#endif
}