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
/// Handles events related to the initialization and update of file and URL multi-analyses,
/// and manages saga transitions and completion.
/// </summary>
internal sealed class MessageAnalysisUpdateStateMachine : MassTransitStateMachine<MessageAnalysisUpdateSaga>
{
    /// <summary>
    /// Gets state representing the initialization phase of the saga.
    /// </summary>
    public State Initialiazing { get; }

    /// <summary>
    /// Gets state representing the in-progress phase of the saga.
    /// </summary>
    public State InProgress { get; }

    /// <summary>
    /// Gets event triggered when a MessageAnalysis is started.
    /// </summary>
    public Event<MessageAnalysisStarted> Started { get; }

    /// <summary>
    /// Gets event triggered when a MessageAnalysis is initialized.
    /// </summary>
    public Event<MessageAnalysisInitialized> Initialized { get; }

    /// <summary>
    /// Gets event triggered when a FileMultiAnalysis is updated.
    /// </summary>
    public Event<UpdateMultiAnalysisMessage<FileAnalysis>> FileMultiAnalysisUpdated { get; }

    /// <summary>
    /// Gets event triggered when a UrlMultiAnalysis is updated.
    /// </summary>
    public Event<UpdateMultiAnalysisMessage<UrlAnalysis>> UrlMultiAnalysisUpdated { get; }

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

        InstanceState(x => x.CurrentState, Initialiazing, InProgress);

        Event(() => Started, e => e.CorrelateById(x => x.Message.CorrelationId.Value));
        Event(() => Initialized, e => e.CorrelateById(x => x.Message.CorrelationId.Value));
        Event(() => FileMultiAnalysisUpdated, e => e.CorrelateBy((saga, context) =>
            context.Message.CorrelationId != null && saga.CorrelationId == context.Message.CorrelationId.Value));
        Event(() => UrlMultiAnalysisUpdated, e => e.CorrelateBy((saga, context) =>
            context.Message.CorrelationId != null && saga.CorrelationId == context.Message.CorrelationId.Value));

        Initially(
            When(Started).Then(context =>
                {
                    _logger.LogDebug(
                        "Saga {CorrelationId}: Started MessageAnalysis with ID {MessageAnalysisId}",
                        context.Message.CorrelationId,
                        context.Message.MessageAnalysisId);
                    context.Saga.CorrelationId = context.Message.CorrelationId.Value;
                    context.Saga.MessageAnalysisId = context.Message.MessageAnalysisId;
                })
                .TransitionTo(Initialiazing));

        During(
            Initialiazing,
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
            When(Initialized).ThenAsync(UpdateDeferredMultiAnalysesAsync).TransitionTo(InProgress));

        During(
            InProgress,
            When(FileMultiAnalysisUpdated).ThenAsync(async context =>
            {
                _logger.LogDebug(
                    "Saga {CorrelationId}: Received FileMultiAnalysis update for {MultiAnalysisId}",
                    context.Saga.CorrelationId,
                    context.Message.MultiAnalysisId);
                await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
                FileMultiAnalysis multiAnalysis = await GetFileMultiAnalysisAsync(
                    scope.ServiceProvider,
                    context.Message.MultiAnalysisId,
                    context.CancellationToken);
                await UpdateAsync(scope.ServiceProvider, multiAnalysis, context);
                _logger.LogDebug(
                    "Saga {CorrelationId}: Finished FileMultiAnalysis update for {MultiAnalysisId}",
                    context.Saga.CorrelationId,
                    context.Message.MultiAnalysisId);
            }),
            When(UrlMultiAnalysisUpdated).ThenAsync(async context =>
            {
                _logger.LogDebug(
                    "Saga {CorrelationId}: Received UrlMultiAnalysis update for {MultiAnalysisId}",
                    context.Saga.CorrelationId,
                    context.Message.MultiAnalysisId);
                await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
                UrlMultiAnalysis multiAnalysis =
                    await GetUrlMultiAnalysisAsync(
                        scope.ServiceProvider,
                        context.Message.MultiAnalysisId,
                        context.CancellationToken);
                await UpdateAsync(scope.ServiceProvider, multiAnalysis, context);
                _logger.LogDebug(
                    "Saga {CorrelationId}: Finished UrlMultiAnalysis update for {MultiAnalysisId}",
                    context.Saga.CorrelationId,
                    context.Message.MultiAnalysisId);
            }));

        SetCompletedWhenFinalized();
    }

    private async Task UpdateDeferredMultiAnalysesAsync(BehaviorContext<MessageAnalysisUpdateSaga> context)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();

        foreach (var deferredId in context.Saga.DeferredFileUpdates)
        {
            _logger.LogDebug(
                "Saga {CorrelationId}: Updating MessageAnalysis from deferred FileMultiAnalysis {MultiAnalysisId}",
                context.Saga.CorrelationId,
                deferredId);
            FileMultiAnalysis multiAnalysis =
                await GetFileMultiAnalysisAsync(scope.ServiceProvider, deferredId, context.CancellationToken);
            await UpdateAsync(scope.ServiceProvider, multiAnalysis, context);
        }

        foreach (var deferredId in context.Saga.DeferredUrlUpdates)
        {
            _logger.LogDebug(
                "Saga {CorrelationId}: Updating MessageAnalysis from deferred UrlMultiAnalysis {MultiAnalysisId}",
                context.Saga.CorrelationId,
                deferredId);
            UrlMultiAnalysis multiAnalysis =
                await GetUrlMultiAnalysisAsync(scope.ServiceProvider, deferredId, context.CancellationToken);
            await UpdateAsync(scope.ServiceProvider, multiAnalysis, context);
        }
    }

    private async Task<FileMultiAnalysis> GetFileMultiAnalysisAsync(
        IServiceProvider serviceProvider,
        GlobalId multiAnalysisId,
        CancellationToken cancellationToken)
    {
        var repository = serviceProvider.GetRequiredService<IRepository<FileMultiAnalysis, GlobalId>>();
        FileMultiAnalysis? fileMultiAnalysis = await repository.GetAsync(multiAnalysisId, cancellationToken);
        if (fileMultiAnalysis is null)
        {
            throw new InvalidOperationException($"FileMultiAnalysis not found for ID: {multiAnalysisId}");
        }

        return fileMultiAnalysis;
    }

    private async Task<UrlMultiAnalysis> GetUrlMultiAnalysisAsync(
        IServiceProvider serviceProvider,
        GlobalId multiAnalysisId,
        CancellationToken cancellationToken)
    {
        var repository = serviceProvider.GetRequiredService<IRepository<UrlMultiAnalysis, GlobalId>>();
        UrlMultiAnalysis? urlMultiAnalysis = await repository.GetAsync(multiAnalysisId, cancellationToken);
        if (urlMultiAnalysis is null)
        {
            throw new InvalidOperationException($"UrlMultiAnalysis not found for ID: {multiAnalysisId}");
        }

        return urlMultiAnalysis;
    }

    private async Task UpdateAsync<TAnalysis>(
        IServiceProvider serviceProvider,
        MultiAnalysis<TAnalysis> multiAnalysis,
        BehaviorContext<MessageAnalysisUpdateSaga> context)
        where TAnalysis : Analysis
    {
        var repository = serviceProvider.GetRequiredService<IRepository<MessageAnalysis, GlobalId>>();
        IReadOnlyList<MessageAnalysis> messageAnalyses = await repository.GetManyAsync(
            page: 0,
            pageSize: 1,
            filter: m =>
                m.AttachedFilesIndicators.Any(i => i.ResultId == multiAnalysis.Id)
                || m.DetectedUrlsIndicators.Any(i => i.ResultId == multiAnalysis.Id),
            orderBy: null,
            context.CancellationToken);

        if (messageAnalyses.Count is 0)
        {
            throw new InvalidOperationException($"No matching MessageAnalysis found for the provided MultiAnalysis ID: {multiAnalysis.Id}");
        }

        MessageAnalysis messageAnalysis = messageAnalyses[0];
        messageAnalysis.UpdateResult(multiAnalysis);
        await repository.UpdateAsync(messageAnalysis, context.CancellationToken);

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