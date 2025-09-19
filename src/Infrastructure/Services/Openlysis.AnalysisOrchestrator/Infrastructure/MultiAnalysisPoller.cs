using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.AnalysisOrchestrator.Abstractions;
using Openlysis.AnalysisOrchestrator.Configuration;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Models;
using Openlysis.Analyzers.Shared.Contracts.Common.Requests;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.AnalysisOrchestrator.Infrastructure;

/// <summary>
/// Polls multiple analyses of type <typeparamref name="TAnalysis"/> using analyzers and requests of type <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TAnalysis">The analysis entity type, must inherit from <see cref="Analysis"/>.</typeparam>
/// <typeparam name="TRequest">The analysis request type, must inherit from <see cref="AnalyzeRequest"/>.</typeparam>
internal sealed class MultiAnalysisPoller<TAnalysis, TRequest> : IMultiAnalysisPoller<TAnalysis>
    where TAnalysis : Analysis
    where TRequest : AnalyzeRequest
{
    /// <inheritdoc/>
    public ChannelReader<TAnalysis[]> UpdatedAnalyses => _updatedAnalysesChannel.Reader;

    private readonly ILogger<MultiAnalysisPoller<TAnalysis, TRequest>> _logger;
    private readonly IOptions<PollingOptions> _pollingOptions;
    private readonly IReadOnlyDictionary<string, Analyzer<TAnalysis, TRequest>> _analyzers;
    private readonly Channel<TAnalysis[]> _updatedAnalysesChannel;
    private readonly ConcurrentDictionary<GlobalId, TAnalysis> _pendingAnalyses = [];
    private readonly ConcurrentBag<TAnalysis> _updatableAnalyses = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalysisPoller{TAnalysis, TRequest}"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging poller events.</param>
    /// <param name="pollingOptions">Polling configuration options.</param>
    /// <param name="analyzers">Dictionary of analyzers for each service.</param>
    /// <param name="channel">Channel for publishing updated analyses.</param>
    public MultiAnalysisPoller(
        ILogger<MultiAnalysisPoller<TAnalysis, TRequest>> logger,
        IOptions<PollingOptions> pollingOptions,
        IReadOnlyDictionary<string, Analyzer<TAnalysis, TRequest>> analyzers,
        Channel<TAnalysis[]> channel)
    {
        _logger = logger;
        _pollingOptions = pollingOptions;
        _analyzers = analyzers;
        _updatedAnalysesChannel = channel;
    }

    /// <inheritdoc/>
    public async Task PollAsync(TAnalysis[] analysisIdentities, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var identity in analysisIdentities)
            {
                _ = _pendingAnalyses.TryAdd(identity.Id, identity);
            }

            await PollAnalysesAsync(cancellationToken);
            _updatedAnalysesChannel.Writer.Complete();
        }
        catch (Exception exception)
        {
            _updatedAnalysesChannel.Writer.Complete(exception);
            throw;
        }
    }

    private async Task PollAnalysesAsync(CancellationToken cancellationToken)
    {
        while (!_pendingAnalyses.IsEmpty)
        {
            await Task.Delay(_pollingOptions.Value.BatchIntervalMs, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await ExecutePollBatchAsync(cancellationToken);
        }
    }

    private async Task ExecutePollBatchAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < _pollingOptions.Value.CyclesPerBatch; i++)
        {
            await Parallel.ForEachAsync(
                _pendingAnalyses.Values,
                cancellationToken,
                PollAnalysisAsync);

            if (!_updatableAnalyses.IsEmpty)
            {
                await _updatedAnalysesChannel.Writer.WriteAsync(
                    _updatableAnalyses.ToArray(),
                    cancellationToken);
                _updatableAnalyses.Clear();
            }

            if (_pendingAnalyses.IsEmpty)
            {
                break;
            }

            await Task.Delay(_pollingOptions.Value.CycleIntervalMs, cancellationToken);
        }
    }

    private async ValueTask PollAnalysisAsync(
        TAnalysis pendingAnalysis,
        CancellationToken cancellationToken)
    {
        var analyzer = _analyzers[pendingAnalysis.ExternalId.Service];

        if (await IsAnalysisFinishedAsync(analyzer, pendingAnalysis, cancellationToken))
        {
            await UpdateFinishedAnalysisAsync(analyzer, pendingAnalysis, cancellationToken);
        }
    }

    private async Task<bool> IsAnalysisFinishedAsync(
        Analyzer<TAnalysis, TRequest> analyzer,
        TAnalysis analysis,
        CancellationToken cancellationToken)
    {
        if (!analyzer.CanGetAnalysisStatus)
        {
            _logger.LogInformation(
                "Service {ServiceName}: Unable to get analysis status for analysis ID {AnalysisId}. Service unavailable.",
                analyzer.ServiceName,
                analysis.Id);
            return false;
        }

        ErrorOr<AnalysisStatus> getStatusResult =
            await analyzer.GetStatusAsync(analysis.ExternalId, cancellationToken);

        if (getStatusResult.IsError)
        {
            _logger.LogError(
                "Service {ServiceName}: Failed to get status for analysis ID {AnalysisId}."
                + "\n\tErrors: {Errors}",
                analyzer.ServiceName,
                analysis.Id,
                getStatusResult.Errors);
            return false;
        }

        return getStatusResult.Value is AnalysisStatus.Completed or AnalysisStatus.Failed or AnalysisStatus.Timeout;
    }

    private async Task UpdateFinishedAnalysisAsync(
        Analyzer<TAnalysis, TRequest> analyzer,
        TAnalysis pendingAnalysis,
        CancellationToken cancellationToken)
    {
        if (!analyzer.CanGetAnalysis)
        {
            _logger.LogInformation(
                "Service {ServiceName}: Unable to get analysis result for analysis ID {AnalysisId}. Service unavailable.",
                analyzer.ServiceName,
                pendingAnalysis.Id);
            return;
        }

        var identity = AnalysisIdentity.FromAnalysis(pendingAnalysis);
        ErrorOr<TAnalysis> getAnalysisResult = await analyzer.GetAnalysisAsync(identity, cancellationToken);
        if (getAnalysisResult.IsError)
        {
            _logger.LogError(
                "Service {ServiceName}: Failed to get analysis result for analysis ID {AnalysisId}."
                + "\n\tErrors: {Errors}",
                analyzer.ServiceName,
                pendingAnalysis.Id,
                getAnalysisResult.Errors);
            return;
        }

        TAnalysis finishedAnalysis = getAnalysisResult.Value;
        if (pendingAnalysis != finishedAnalysis)
        {
            throw new InvalidOperationException(
                $"Pending analysis with ID {pendingAnalysis.Id} does not match the finished analysis with ID {finishedAnalysis.Id}.");
        }

        _pendingAnalyses.TryRemove(finishedAnalysis.Id, out _);
        _updatableAnalyses.Add(finishedAnalysis);
    }
}