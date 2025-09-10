using MassTransit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.Messages;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Sagas.Messages;

/// <summary>
/// Represents the MassTransit state machine for updating the state of a MessageAnalysis.
/// Handles events related to the initialization and update of related file and URL multi-analyses,
/// and manages saga transitions and completion.
/// </summary>
internal sealed class MessageAnalysisUpdateStateMachine : MassTransitStateMachine<MessageAnalysisUpdateSaga>
{
    /// <summary>
    /// Gets state representing the initialization phase of the saga.
    /// </summary>
    public State Initializing { get; } = null!;

    /// <summary>
    /// Gets state representing the in-progress phase of the saga.
    /// </summary>
    public State InProgress { get; } = null!;

    /// <summary>
    /// Gets event triggered when a MessageAnalysis is started.
    /// </summary>
    public Event<MessageAnalysisStarted> Started { get; } = null!;

    /// <summary>
    /// Gets event triggered when a MessageAnalysis is initialized.
    /// </summary>
    public Event<MessageAnalysisInitialized> Initialized { get; } = null!;

    /// <summary>
    /// Gets event triggered when a FileMultiAnalysis is updated.
    /// </summary>
    public Event<UpdateMultiAnalysisMessage<FileAnalysis>> FileMultiAnalysisUpdated { get; } = null!;

    /// <summary>
    /// Gets event triggered when a UrlMultiAnalysis is updated.
    /// </summary>
    public Event<UpdateMultiAnalysisMessage<UrlAnalysis>> UrlMultiAnalysisUpdated { get; } = null!;

    private readonly ILogger<MessageAnalysisUpdateStateMachine> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisUpdateStateMachine"/> class.
    /// </summary>
    /// <param name="logger">Logger for saga state machine events.</param>
    /// <param name="serviceScopeFactory">Factory to create service scopes for dependency resolution.</param>
    public MessageAnalysisUpdateStateMachine(
        ILogger<MessageAnalysisUpdateStateMachine> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        InstanceState(x => x.CurrentState, Initializing, InProgress);

        Event(() => Started, e => e.CorrelateById(x => x.Message.CorrelationId.Value));
        Event(() => Initialized, e => e.CorrelateById(x => x.Message.CorrelationId.Value));
        Event(() => FileMultiAnalysisUpdated, e => e.CorrelateBy((saga, context) =>
            context.Message.CorrelationId != null && saga.CorrelationId == context.Message.CorrelationId.Value));
        Event(() => UrlMultiAnalysisUpdated, e => e.CorrelateBy((saga, context) =>
            context.Message.CorrelationId != null && saga.CorrelationId == context.Message.CorrelationId.Value));

        Initially(When(Started).Then(SetUpSagaInformation).TransitionTo(Initializing));

        During(
            Initializing,
            When(FileMultiAnalysisUpdated).Then(context =>
            {
                _logger.LogDebug(
                    "Saga {CorrelationId}: Deferring FileMultiAnalysis update for {MultiAnalysisId}",
                    context.Saga.CorrelationId,
                    context.Message.MultiAnalysisId);
                context.Saga.DeferredFileUpdates.Add(context.Message.MultiAnalysisId);
            }),
            When(UrlMultiAnalysisUpdated).Then(context =>
            {
                _logger.LogDebug(
                    "Saga {CorrelationId}: Deferring UrlMultiAnalysis update for {MultiAnalysisId}",
                    context.Saga.CorrelationId,
                    context.Message.MultiAnalysisId);
                context.Saga.DeferredUrlUpdates.Add(context.Message.MultiAnalysisId);
            }),
            When(Initialized).ThenAsync(ApplyDeferredUpdatesAsync).TransitionTo(InProgress));

        During(
            InProgress,
            When(FileMultiAnalysisUpdated).ThenAsync(ApplyUpdateAsync<FileMultiAnalysis, FileAnalysis>),
            When(UrlMultiAnalysisUpdated).ThenAsync(ApplyUpdateAsync<UrlMultiAnalysis, UrlAnalysis>));

        SetCompletedWhenFinalized();
    }

    private static async Task<MessageAnalysis> GetRequiredAnalysisAsync(
        IRepository<MessageAnalysis> repository,
        BehaviorContext<MessageAnalysisUpdateSaga> context)
    {
        GlobalId messageAnalysisId = context.Saga.MessageAnalysisId;
        MessageAnalysis? messageAnalysis = await repository.GetAsync(messageAnalysisId, context.CancellationToken);
        return messageAnalysis
            ?? throw new InvalidOperationException($"MessageAnalysis entity not found. ID: {messageAnalysisId}, Saga CorrelationId: {context.Saga.CorrelationId}");
    }

    private static async Task<TMultiAnalysis> GetRequiredMultiAnalysisAsync<TMultiAnalysis, TAnalysis>(
        IServiceProvider serviceProvider,
        GlobalId id,
        CancellationToken cancellationToken)
        where TMultiAnalysis : MultiAnalysis<TAnalysis>
        where TAnalysis : Analysis
    {
        var repository = serviceProvider.GetRequiredService<IRepository<TMultiAnalysis>>();
        TMultiAnalysis? multiAnalysis = await repository.GetAsync(id, cancellationToken);
        return multiAnalysis ?? throw new InvalidOperationException($"{typeof(TMultiAnalysis).Name} not found for ID: {id}");
    }

    private void SetUpSagaInformation(BehaviorContext<MessageAnalysisUpdateSaga, MessageAnalysisStarted> context)
    {
        _logger.LogDebug(
            "Saga {CorrelationId}: Started MessageAnalysis with ID {MessageAnalysisId}",
            context.Message.CorrelationId,
            context.Message.MessageAnalysisId);
        context.Saga.CorrelationId = context.Message.CorrelationId.Value;
        context.Saga.MessageAnalysisId = context.Message.MessageAnalysisId;
    }

    private async Task ApplyDeferredUpdatesAsync(BehaviorContext<MessageAnalysisUpdateSaga> context)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<MessageAnalysis>>();
        MessageAnalysis messageAnalysis = await GetRequiredAnalysisAsync(repository, context);

        foreach (var deferredId in context.Saga.DeferredFileUpdates)
        {
            _logger.LogDebug(
                "Saga {CorrelationId}: Updating MessageAnalysis from deferred FileMultiAnalysis {MultiAnalysisId}",
                context.Saga.CorrelationId,
                deferredId);

            var multiAnalysis = await GetRequiredMultiAnalysisAsync<FileMultiAnalysis, FileAnalysis>(
                scope.ServiceProvider,
                deferredId,
                context.CancellationToken);
            messageAnalysis.UpdateResult(multiAnalysis);
        }

        foreach (var deferredId in context.Saga.DeferredUrlUpdates)
        {
            _logger.LogDebug(
                "Saga {CorrelationId}: Updating MessageAnalysis from deferred UrlMultiAnalysis {MultiAnalysisId}",
                context.Saga.CorrelationId,
                deferredId);

            var multiAnalysis = await GetRequiredMultiAnalysisAsync<UrlMultiAnalysis, UrlAnalysis>(
                scope.ServiceProvider,
                deferredId,
                context.CancellationToken);
            messageAnalysis.UpdateResult(multiAnalysis);
        }

        repository.Update(messageAnalysis);
        await repository.SaveChangeAsync(context.CancellationToken);
        await CompleteSagaIfFinishedAsync(messageAnalysis, context);
    }

    private async Task ApplyUpdateAsync<TMultiAnalysis, TAnalysis>(
        BehaviorContext<MessageAnalysisUpdateSaga, UpdateMultiAnalysisMessage<TAnalysis>> context)
        where TMultiAnalysis : MultiAnalysis<TAnalysis>
        where TAnalysis : Analysis
    {
        _logger.LogDebug(
            "Saga {CorrelationId}: Received {MultiAnalysisType} update with ID {MultiAnalysisId}",
            context.Saga.CorrelationId,
            typeof(TMultiAnalysis).Name,
            context.Message.MultiAnalysisId);

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<MessageAnalysis>>();
        TMultiAnalysis multiAnalysis = await GetRequiredMultiAnalysisAsync<TMultiAnalysis, TAnalysis>(
            scope.ServiceProvider,
            context.Message.MultiAnalysisId,
            context.CancellationToken);
        MessageAnalysis messageAnalysis = await GetRequiredAnalysisAsync(repository, context);
        messageAnalysis.UpdateResult(multiAnalysis);
        repository.Update(messageAnalysis);
        await repository.SaveChangeAsync(context.CancellationToken);
        await CompleteSagaIfFinishedAsync(messageAnalysis, context);

        _logger.LogDebug(
            "Saga {CorrelationId}: Finished {MultiAnalysisType} update with ID {MultiAnalysisId}",
            context.Saga.CorrelationId,
            typeof(TMultiAnalysis).Name,
            context.Message.MultiAnalysisId);
    }

    private async Task CompleteSagaIfFinishedAsync(
        MessageAnalysis messageAnalysis,
        BehaviorContext<MessageAnalysisUpdateSaga> context)
    {
        if (messageAnalysis.State.CanBeUpdated)
        {
            return;
        }

        await context.SetCompleted();

        _logger.LogDebug(
            "Saga {CorrelationId}: State machine completed for MessageAnalysis {MessageAnalysisId}",
            context.Saga.CorrelationId,
            context.Saga.MessageAnalysisId);
    }
}