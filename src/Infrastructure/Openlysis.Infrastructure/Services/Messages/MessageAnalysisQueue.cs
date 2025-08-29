using MassTransit;

using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;
using Openlysis.Infrastructure.Communication.Sagas.Messages;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// Represents a queue for handling message analysis operations.
/// </summary>
internal sealed class MessageAnalysisQueue : IMessageAnalysisQueue
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisQueue"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">Provides URIs for message endpoints.</param>
    /// <param name="sendEndpointProvider">Provides endpoints for sending messages.</param>
    public MessageAnalysisQueue(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider)
    {
        _endpointUriProvider = endpointUriProvider;
        _sendEndpointProvider = sendEndpointProvider;
    }

    /// <inheritdoc/>
    public async Task EnqueueAsync(
        GlobalId correlationId,
        MessageAnalysis analysis,
        CancellationToken cancellationToken = default)
    {
        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.MessageAnalysisUpdateUri);
        var request = new MessageAnalysisStarted(correlationId, analysis.Id);
        await sendEndpoint.Send(request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SetAsInitializedAsync(
        GlobalId correlationId,
        CancellationToken cancellationToken = default)
    {
        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.MessageAnalysisUpdateUri);
        var request = new MessageAnalysisInitialized(correlationId);
        await sendEndpoint.Send(request, cancellationToken);
    }
}