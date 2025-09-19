using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.AnalysisOrchestrator.Configuration;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Common.Models;
using Openlysis.Analyzers.Shared.Contracts.Common.Requests;
using Openlysis.Domain.Common.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.AnalysisOrchestrator.Abstractions;

/// <summary>
/// Abstract base class for managing multiple analyzers of type <typeparamref name="TAnalysis"/>.
/// </summary>
/// <typeparam name="TAnalysis">The analysis entity type.</typeparam>
/// <typeparam name="TMessage">The analysis job message type.</typeparam>
/// <typeparam name="TRequest">The analysis request type.</typeparam>
internal abstract class MultiAnalyzer<TAnalysis, TMessage, TRequest>
    : IMultiAnalyzer<TAnalysis, TMessage>
    where TAnalysis : Analysis
    where TMessage : AnalysisJobMessage
    where TRequest : AnalyzeRequest
{
    /// <inheritdoc/>
    public ChannelReader<TAnalysis> StartedAnalyses => _startedAnalysesChannel.Reader;

    private readonly ILogger<MultiAnalyzer<TAnalysis, TMessage, TRequest>> _logger;
    private readonly IOptions<MultiAnalyzerOptions> _options;
    private readonly IReadOnlyDictionary<string, Analyzer<TAnalysis, TRequest>> _analyzers;
    private readonly Channel<TAnalysis> _startedAnalysesChannel;
    private readonly ConcurrentDictionary<string, Analyzer<TAnalysis, TRequest>> _remainingAnalyzers;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalyzer{TAnalysis, TMessage, TRequest}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging analysis operations.</param>
    /// <param name="options">The options for multi-analyzer configuration.</param>
    /// <param name="analyzers">A dictionary of available analyzers keyed by service name.</param>
    /// <param name="channel">The channel used to communicate started analyses.</param>
    protected MultiAnalyzer(
        ILogger<MultiAnalyzer<TAnalysis, TMessage, TRequest>> logger,
        IOptions<MultiAnalyzerOptions> options,
        IReadOnlyDictionary<string, Analyzer<TAnalysis, TRequest>> analyzers,
        Channel<TAnalysis> channel)
    {
        _logger = logger;
        _options = options;
        _analyzers = analyzers;
        _remainingAnalyzers = new ConcurrentDictionary<string, Analyzer<TAnalysis, TRequest>>(analyzers);
        _startedAnalysesChannel = channel;
    }

    /// <inheritdoc/>
    public async Task AnalyzeAsync(
        TMessage message,
        AnalysisIdentity[] alreadyStartedAnalyses,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ExcludeAlreadyUsedAnalyzers(alreadyStartedAnalyses);
            var request = CreateRequest(message);
            await StartAnalysesAsync(request, cancellationToken);
            await RetryFailedAnalysesAsync(request, cancellationToken);

            if (!_remainingAnalyzers.IsEmpty)
            {
                _logger.LogWarning(
                    "Multi-Analysis completed with {FailureCount} permanently failing analyzers: {AnalyzerNames}",
                    _remainingAnalyzers.Count,
                    string.Join(", ", _remainingAnalyzers.Keys));
            }

            _startedAnalysesChannel.Writer.Complete();
            await CleanUpAsync(success: true);
        }
        catch (Exception exception)
        {
            _startedAnalysesChannel.Writer.Complete(exception);
            await CleanUpAsync(success: false);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TAnalysis>> GetAnalysesByIdAsync(
        AnalysisIdentity[] analysisIdentities,
        CancellationToken cancellationToken = default)
    {
        ConcurrentBag<TAnalysis> analyses = [];

        await Parallel.ForEachAsync(
            analysisIdentities,
            cancellationToken,
            async (identity, ct) =>
            {
                var analyzer = _analyzers[identity.ExternalId.Service];
                var result = await analyzer.GetAnalysisAsync(identity, ct);

                if (result.IsError)
                {
                    _logger.LogWarning(
                        "Service {ServiceName}: Failed to retrieve {AnalysisType} with ID {AnalysisId}."
                        + "\n\tErrors: {Errors}.",
                        analyzer.ServiceName,
                        typeof(TAnalysis),
                        identity.Id,
                        result.Errors);
                    return;
                }

                analyses.Add(result.Value);
            });

        return analyses.ToList();
    }

    /// <summary>
    /// Creates an analysis request from the provided message.
    /// </summary>
    /// <param name="message">The analysis job message to process.</param>
    /// <returns>A new analysis request.</returns>
    protected abstract TRequest CreateRequest(TMessage message);

    /// <summary>
    /// Performs cleanup operations after analysis is complete.
    /// Can be overridden by derived classes to implement custom cleanup logic.
    /// </summary>
    /// <param name="success">Indicates whether the analysis completed successfully.</param>
    /// <returns>A completed <see cref="ValueTask"/> by default.</returns>
    protected virtual ValueTask CleanUpAsync(bool success)
    {
        return ValueTask.CompletedTask;
    }

    private async Task StartAnalysesAsync(TRequest request, CancellationToken cancellationToken)
    {
        ConcurrentBag<Exception> thrownExceptions = [];
        await Parallel.ForEachAsync(
            _remainingAnalyzers.Values,
            cancellationToken,
            async (analyzer, ct) =>
            {
                try
                {
                    await StartAnalysisAsync(request, analyzer, ct);
                }
                catch (Exception exception)
                {
                    thrownExceptions.Add(exception);
                }
            });

        if (!thrownExceptions.IsEmpty)
        {
            throw new AggregateException(thrownExceptions);
        }
    }

    private async ValueTask StartAnalysisAsync(
        TRequest request,
        Analyzer<TAnalysis, TRequest> analyzer,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Analyzing with {ServiceName}", analyzer.ServiceName);

        if (!analyzer.CanAnalyze)
        {
            _logger.LogWarning(
                "Service {ServiceName}: Unable to start {AnalysisType}. Service unavailable.",
                analyzer.ServiceName,
                typeof(TAnalysis));
            return;
        }

        var result = await analyzer.AnalyzeAsync(request, cancellationToken);

        if (result.IsError)
        {
            _logger.LogError(
                "Service {ServiceName}: Failed to start {AnalysisType}.\n\tErrors: {Errors}",
                analyzer.ServiceName,
                typeof(TAnalysis),
                result.Errors);
            return;
        }

        await _startedAnalysesChannel.Writer.WriteAsync(result.Value, cancellationToken);
        _ = _remainingAnalyzers.Remove(analyzer.ServiceName, out _);

        _logger.LogDebug("Analysis started with {ServiceName}", analyzer.ServiceName);
    }

    private async ValueTask RetryFailedAnalysesAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (_remainingAnalyzers.IsEmpty)
        {
            return;
        }

        int attempt = 0;
        using List<int>.Enumerator retryEnumerator = _options.Value.RetryIntervals.GetEnumerator();
        while (!_remainingAnalyzers.IsEmpty && retryEnumerator.MoveNext())
        {
            attempt++;
            _logger.LogInformation(
                "Starting retry attempt {Attempt} for {RemainingCount} analyzers.",
                attempt,
                _remainingAnalyzers.Count);

            var retryInterval = retryEnumerator.Current;
            await Task.Delay(retryInterval, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (_remainingAnalyzers.Count > 1)
            {
                await Parallel.ForEachAsync(
                    _remainingAnalyzers.Values,
                    cancellationToken,
                    async (analyzer, ct) => await StartAnalysisAsync(request, analyzer, ct));
            }
            else
            {
                await StartAnalysisAsync(request, _remainingAnalyzers.Values.First(), cancellationToken);
            }
        }
    }

    private void ExcludeAlreadyUsedAnalyzers(AnalysisIdentity[] analysisIdentities)
    {
        foreach (AnalysisIdentity identity in analysisIdentities)
        {
            _ = _remainingAnalyzers.Remove(identity.ExternalId.Service, out _);
        }
    }
}