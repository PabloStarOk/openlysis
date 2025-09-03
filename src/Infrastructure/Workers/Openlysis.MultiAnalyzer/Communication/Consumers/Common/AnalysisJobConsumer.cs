using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Openlysis.Analyzers.Shared.Contracts.Common.Models;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.Common;

/// <summary>
/// Consumes analysis job messages and manages the lifecycle of analysis jobs.
/// </summary>
/// <typeparam name="TAnalysis">Type representing an analysis entity.</typeparam>
/// <typeparam name="TMessage">Type representing the analysis job message.</typeparam>
internal sealed class AnalysisJobConsumer<TAnalysis, TMessage> : IJobConsumer<TMessage>
    where TAnalysis : Analysis
    where TMessage : AnalysisJobMessage
{
    private readonly ILogger<AnalysisJobConsumer<TAnalysis, TMessage>> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IMultiAnalyzer<TAnalysis, TMessage> _multiAnalyzer;
    private readonly IMultiAnalysisPoller<TAnalysis> _multiAnalysisPoller;
    private readonly IUpdateMessageSender _updateMessageSender;
    private readonly HashSet<TAnalysis> _startedAnalyses = [];
    private readonly int _progressLimit = Enum.GetValues<AnalysisJobConsumerStatus>().Length;
    private JobContext<TMessage> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisJobConsumer{TAnalysis, TMessage}"/> class.
    /// </summary>
    /// <param name="logger">Logger for job consumer events.</param>
    /// <param name="appLifetime">Application lifetime for shutdown and cancellation handling.</param>
    /// <param name="multiAnalyzer">Service to manage and execute analyses.</param>
    /// <param name="multiAnalysisPoller">Service to poll analysis results.</param>
    /// <param name="updateMessageSender">Service to send update messages about analysis progress.</param>
    public AnalysisJobConsumer(
        ILogger<AnalysisJobConsumer<TAnalysis, TMessage>> logger,
        IHostApplicationLifetime appLifetime,
        IMultiAnalyzer<TAnalysis, TMessage> multiAnalyzer,
        IMultiAnalysisPoller<TAnalysis> multiAnalysisPoller,
        IUpdateMessageSender updateMessageSender)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _multiAnalyzer = multiAnalyzer;
        _multiAnalysisPoller = multiAnalysisPoller;
        _updateMessageSender = updateMessageSender;
    }

    /// <inheritdoc/>
    public async Task Run(JobContext<TMessage> context)
    {
        try
        {
            _logger.LogInformation("Job {JobId}: Executing.", context.JobId);
            _logger.LogInformation("Job {JobId}: Current attempt {Attempt}.", context.JobId, context.RetryAttempt);
            _context = context;
            await ExecuteJobAsync(context.CancellationToken);
        }
        catch (OperationCanceledException) when (!_appLifetime.ApplicationStopping.IsCancellationRequested)
        {
            _logger.LogWarning("Job {JobId}: Timed out.", context.JobId);
            await SendUpdateMessageAsync(updatableAnalyses: [], timeout: true, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job {JobId}: Stopped.", context.JobId);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Job {JobId}: An unexpected error occurred.", context.JobId);
            throw;
        }
    }

    private async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        AnalysisJobConsumerState state;

        if (_context.TryGetJobState(out AnalysisJobConsumerState existingState))
        {
            state = existingState;
            _logger.LogTrace(
                "Job {JobId}: Existing state retrieved."
                + "\n\tState status: {StateStatus}"
                + "\n\tStarted Analysis Identities: {Identities}",
                _context.JobId,
                state.Status,
                state.StartedAnalysisIdentities);
        }
        else
        {
            state = new AnalysisJobConsumerState
            {
                Status = AnalysisJobConsumerStatus.Started,
            };
            _logger.LogDebug("Job {JobId}: State created.", _context.JobId);
        }

        if (state.Status is AnalysisJobConsumerStatus.Started)
        {
            _logger.LogDebug("Job {JobId}: Started.", _context.JobId);

            await StartAnalysesAsync(state, cancellationToken);
            state.Status = AnalysisJobConsumerStatus.ReportingStart;
            await _context.SaveJobState(state);
            await _context.SetJobProgress((int)state.Status, _progressLimit);

            _logger.LogDebug("Job {JobId}: Analyses started.", _context.JobId);
        }

        if (state.Status is AnalysisJobConsumerStatus.ReportingStart)
        {
            _logger.LogDebug("Job {JobId}: Reporting started analyses.", _context.JobId);

            await EnsureAnalysesAreLoadedAsync(state, cancellationToken);
            await SendUpdateMessageAsync(_startedAnalyses.ToArray(), timeout: false, cancellationToken);
            state.Status = AnalysisJobConsumerStatus.Polling;
            await _context.SaveJobState(state);
            await _context.SetJobProgress((int)state.Status, _progressLimit);

            _logger.LogDebug("Job {JobId}: Started analyses reported.", _context.JobId);
        }

        if (state.Status is AnalysisJobConsumerStatus.Polling)
        {
            _logger.LogDebug("Job {JobId}: Polling analyses.", _context.JobId);

            await EnsureAnalysesAreLoadedAsync(state, cancellationToken);
            await PollAnalysesAsync(cancellationToken);
            state.Status = AnalysisJobConsumerStatus.Completed;
            await _context.SaveJobState(state);
            await _context.SetJobProgress((int)state.Status, _progressLimit);

            _logger.LogDebug("Job {JobId}: Analyses polled.", _context.JobId);
        }

        _logger.LogDebug("Job {JobId}: Completed.", _context.JobId);
    }

    private async Task StartAnalysesAsync(
        AnalysisJobConsumerState state,
        CancellationToken cancellationToken)
    {
        Task analysisTask = _multiAnalyzer.AnalyzeAsync(
            _context.Job,
            state.StartedAnalysisIdentities.ToArray(),
            cancellationToken);

        await foreach (TAnalysis analysis
                       in _multiAnalyzer.StartedAnalyses.ReadAllAsync(cancellationToken))
        {
            var identity = AnalysisIdentity.FromAnalysis(analysis);
            state.StartedAnalysisIdentities.Add(identity);
            await _context.SaveJobState(state);
            _startedAnalyses.Add(analysis);
            _logger.LogDebug(
                "Job {JobId}: Added new analysis identity, updated collection: {Identities}",
                _context.JobId,
                state.StartedAnalysisIdentities);
        }

        await analysisTask;
    }

    private async Task PollAnalysesAsync(CancellationToken cancellationToken)
    {
        if (_startedAnalyses.All(a => a.State.Status is AnalysisStatus.Completed))
        {
            await SendUpdateMessageAsync(_startedAnalyses.ToArray(), timeout: false, cancellationToken);
            return;
        }

        Task pollingTask = _multiAnalysisPoller.PollAsync(_startedAnalyses.ToArray(), cancellationToken);

        await foreach (TAnalysis[] updatedAnalyses
                       in _multiAnalysisPoller.UpdatedAnalyses.ReadAllAsync(cancellationToken))
        {
            await SendUpdateMessageAsync(updatedAnalyses, timeout: false, cancellationToken);
        }

        await pollingTask;
    }

    private async ValueTask EnsureAnalysesAreLoadedAsync(
        AnalysisJobConsumerState state,
        CancellationToken cancellationToken)
    {
        if (_startedAnalyses.Count == state.StartedAnalysisIdentities.Count)
        {
            return;
        }

        var loadedAnalysisIds = _startedAnalyses.Select(a => a.ExternalId);
        var missingAnalysisIds = state.StartedAnalysisIdentities
            .Where(i => !loadedAnalysisIds.Contains(i.ExternalId));
        var loadedAnalyses = await _multiAnalyzer.GetAnalysesByIdAsync(
            missingAnalysisIds.ToArray(),
            cancellationToken);
        foreach (var loadedAnalysis in loadedAnalyses)
        {
            _startedAnalyses.Add(loadedAnalysis);
        }

        _logger.LogDebug("Job {JobId}: {Count} missing analyses loaded.", _context.JobId, loadedAnalyses.Count);
    }

    private async Task SendUpdateMessageAsync(
        TAnalysis[] updatableAnalyses,
        bool timeout,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Job {JobId}: Sending update message.", _context.JobId);

        var message = new UpdateMultiAnalysisMessage<TAnalysis>(
            _context.Job.MultiAnalysisId,
            timeout,
            updatableAnalyses,
            _context.Job.CorrelationId);

#if DEBUG
        LogUpdateMessage(message, updatableAnalyses);
#endif

        await _updateMessageSender.SendAsync(_context, message, cancellationToken);

        _logger.LogDebug("Job {JobId}: Update message sent.", _context.JobId);
    }

#if DEBUG
    private void LogUpdateMessage(
        UpdateMultiAnalysisMessage<TAnalysis> updateMessage,
        TAnalysis[] updatableAnalyses)
    {
        string analysesLog;
        if (updatableAnalyses.Length > 0)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            foreach (var analysis in updatableAnalyses)
            {
                stringBuilder.AppendLine("\t{");
                stringBuilder.AppendLine($"\t\tAnalysis ID: {analysis.Id}");
                stringBuilder.AppendLine($"\t\tAnalysis external primary ID: {analysis.ExternalId.Primary}");
                stringBuilder.AppendLine($"\t\tAnalysis external job ID: {analysis.ExternalId.Job}");
                stringBuilder.AppendLine($"\t\tAnalysis service: {analysis.ExternalId.Service}");
                stringBuilder.AppendLine($"\t\tAnalysis status: {analysis.State.Status}");
                stringBuilder.AppendLine($"\t\tAnalysis verdict: {analysis.State.Verdict}");
                stringBuilder.AppendLine($"\t\tAnalysis threat score: {analysis.ThreatScore}");
                stringBuilder.AppendLine("\t}");
            }

            analysesLog = stringBuilder.ToString();
        }
        else
        {
            analysesLog = "(empty)";
        }

        _logger.LogTrace(
            "Job {JobId}: Update message"
            + "\n\tMultiAnalysis ID: {MultiAnalysisId}"
            + "\n\tIs timeout: {Timeout}"
            + "\n\tCorrelation ID: {CorrelationId}"
            + "\n\tUpdatable analyses: {AnalysesLog}",
            _context.JobId,
            updateMessage.MultiAnalysisId,
            updateMessage.Timeout,
            updateMessage.CorrelationId,
            analysesLog);
    }
#endif
}