using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Configuration;

namespace Openlysis.MultiAnalyzer.Abstractions;

/// <summary>
/// Orchestrates the analysis workflow for a specific analysis and request type.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis being orchestrated, must inherit from <see cref="Analysis"/>.</typeparam>
/// <typeparam name="TRequest">The type of request message, must be a reference type.</typeparam>
internal abstract class MultiAnalysisOrchestrator<TAnalysis, TRequest>
    : IRequestOrchestrator<TRequest>
    where TAnalysis : Analysis
    where TRequest : class
{
    /// <summary>
    /// Gets the logger instance for logging operations within the analysis orchestrator.
    /// </summary>
    protected ILogger<MultiAnalysisOrchestrator<TAnalysis, TRequest>> Logger { get; }

    /// <summary>
    /// Gets the collection of analyses that are pending processing, keyed by their composed analysis ID.
    /// </summary>
    protected ConcurrentDictionary<ComposedAnalysisId, TAnalysis> PendingAnalyses { get; } = [];

    /// <summary>
    /// Gets the collection of analyses that are ready to be updated.
    /// </summary>
    protected ConcurrentBag<TAnalysis> UpdatableAnalyses { get; } = [];

    /// <summary>
    /// Gets or sets the global identifier for the multi-analysis operation.
    /// </summary>
    protected GlobalId MultiAnalysisId { get; set; }

    private readonly IOptions<OrchestrationOptions> _options;
    private readonly IUpdateMessageSender<UpdateMultiAnalysis<TAnalysis>> _updateMessageSender;
    private ConsumeContext<TRequest> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAnalysisOrchestrator{TAnalysis,TRequest}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="options">The options for configuring the analysis orchestrator.</param>
    /// <param name="updateMessageSender">The sender responsible for dispatching update messages.</param>
    protected MultiAnalysisOrchestrator(
        ILogger<MultiAnalysisOrchestrator<TAnalysis, TRequest>> logger,
        IOptions<OrchestrationOptions> options,
        IUpdateMessageSender<UpdateMultiAnalysis<TAnalysis>> updateMessageSender)
    {
        Logger = logger;
        _options = options;
        _updateMessageSender = updateMessageSender;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(
        ConsumeContext<TRequest> context,
        CancellationToken cancellationToken)
    {
        _context = context;
        await AnalyzeAsync(context.Message, cancellationToken);
        await UpdateAnalysesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task OnTimeoutAsync()
    {
        await SendUpdateAsync(timeout: true, CancellationToken.None);
    }

    /// <summary>
    /// Handles the analysis logic for the given request message.
    /// </summary>
    /// <param name="message">The request message to analyze.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a collection of analysis results.</returns>
    protected abstract Task<IEnumerable<TAnalysis>> HandleAnalyzeAsync(
        TRequest message,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the given analysis instance asynchronously.
    /// </summary>
    /// <param name="analysis">The analysis instance to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous update operation.</returns>
    protected abstract ValueTask UpdateAnalysisAsync(
        TAnalysis analysis,
        CancellationToken cancellationToken);

#if DEBUG
    /// <summary>
    /// Logs the update message for debugging purposes.
    /// </summary>
    /// <param name="message">The update message to log.</param>
    protected abstract void LogUpdateMessage(UpdateMultiAnalysis<TAnalysis> message);

    /// <summary>
    /// Logs the given updatable analysis instance for debugging purposes.
    /// </summary>
    /// <param name="analysis">The analysis instance to log.</param>
    protected abstract void LogUpdatableAnalysis(TAnalysis analysis);
#endif

    /// <summary>
    /// Analyzes data based on the provided <see cref="TRequest"/> and sends update with the initial <see cref="TAnalysis"/> instances.
    /// </summary>
    /// <param name="message">The request message to start the analyses.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous analysis operation.</returns>
    private async Task AnalyzeAsync(
        TRequest message,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<TAnalysis> analyses =
            await HandleAnalyzeAsync(message, cancellationToken);

        foreach (var analysis in analyses)
        {
            PendingAnalyses.TryAdd(analysis.Id, analysis);
            UpdatableAnalyses.Add(analysis);
        }

        await SendUpdateAsync(timeout: false, cancellationToken);
        UpdatableAnalyses.Clear();
    }

    /// <summary>
    /// Continuously updates pending analyses in batches until all are processed.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    private async Task UpdateAnalysesAsync(CancellationToken cancellationToken)
    {
        while (!PendingAnalyses.IsEmpty)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteUpdateBatchAsync(cancellationToken);
            await Task.Delay(_options.Value.UpdateBatchIntervalMs, cancellationToken);
        }
    }

    /// <summary>
    /// Executes a batch of analysis updates in parallel, sending updates and handling delays as configured.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous batch update operation.</returns>
    private async Task ExecuteUpdateBatchAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < _options.Value.UpdateCyclesPerBatch; i++)
        {
            await Parallel.ForEachAsync(
                PendingAnalyses.Values,
                cancellationToken,
                UpdateAnalysisAsync);

            if (!UpdatableAnalyses.IsEmpty)
            {
                await SendUpdateAsync(timeout: false, cancellationToken);

                UpdatableAnalyses.Clear();
            }

            if (PendingAnalyses.IsEmpty)
            {
                break;
            }

            await Task.Delay(_options.Value.UpdateCycleIntervalMs, cancellationToken);
        }
    }

    /// <summary>
    /// Sends an update message containing the current updatable analyses.
    /// </summary>
    /// <param name="timeout">Indicates whether the update is due to a timeout.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task SendUpdateAsync(
        bool timeout,
        CancellationToken cancellationToken = default)
    {
        var message = new UpdateMultiAnalysis<TAnalysis>(
            MultiAnalysisId,
            timeout,
            UpdatableAnalyses);
        await _updateMessageSender.SendAsync(_context, message, cancellationToken);

#if DEBUG
        LogUpdateMessage(message);

        foreach (var analysis in UpdatableAnalyses)
        {
            LogUpdatableAnalysis(analysis);
        }
#endif
    }
}
