using MassTransit;

using Microsoft.Extensions.Logging;

using Openlysis.Domain.Common.Entities;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Communication.Consumers.Common;

/// <summary>
/// MassTransit consumer that handles faulted analysis jobs.
/// </summary>
/// <typeparam name="TAnalysis">Type of the analysis entity.</typeparam>
/// <typeparam name="TMessage">Type of the analysis job message.</typeparam>
internal sealed class FaultedMultiAnalysisConsumer<TAnalysis, TMessage> : IConsumer<Fault<TMessage>>
    where TAnalysis : Analysis
    where TMessage : AnalysisJobMessage
{
    private readonly ILogger<FaultedMultiAnalysisConsumer<TAnalysis, TMessage>> _logger;
    private readonly IEndpointUriProvider _endpointUriProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FaultedMultiAnalysisConsumer{TAnalysis, TMessage}"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging job events.</param>
    /// <param name="endpointUriProvider">Provider for endpoint URIs used to send update messages.</param>
    public FaultedMultiAnalysisConsumer(
        ILogger<FaultedMultiAnalysisConsumer<TAnalysis, TMessage>> logger,
        IEndpointUriProvider endpointUriProvider)
    {
        _logger = logger;
        _endpointUriProvider = endpointUriProvider;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<Fault<TMessage>> context)
    {
        _logger.LogWarning(
            "Job with ID {JobId} faulted for job type {JobType}",
            context.RequestId,
            typeof(TMessage));

        await context.FinalizeJob(context.RequestId ?? Guid.Empty);
        await SendFailedMessageAsync(context);

        _logger.LogDebug("Job with ID {JobId} finalized.", context.RequestId);
    }

    private async Task SendFailedMessageAsync(ConsumeContext<Fault<TMessage>> context)
    {
        Uri endpointUri = context.Message.Message switch
        {
            FileAnalysisJobMessage => _endpointUriProvider.UpdateFileMultiAnalysisUri,
            UrlAnalysisJobMessage => _endpointUriProvider.UpdateUrlMultiAnalysisUri,
            _ => throw new InvalidOperationException("Unknown job message type"),
        };

        ISendEndpoint sendEndpoint = await context.GetSendEndpoint(endpointUri);
        var failedUpdateMessage = new UpdateMultiAnalysisMessage<TAnalysis>(
            context.Message.Message.MultiAnalysisId,
            Timeout: false,
            UpdatableAnalyses: [],
            CorrelationId: null,
            Failed: true);

        await sendEndpoint.Send(failedUpdateMessage, context.CancellationToken);

        _logger.LogDebug("Failed UpdateMessageAnalysis sent for {AnalysisType}.", typeof(TAnalysis));
    }
}