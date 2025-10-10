using MassTransit;

using Openlysis.Application.URLs.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.Infrastructure.Services.URLs;

/// <summary>
/// Provides functionality to queue URLs for multi-analysis processing.
/// </summary>
internal class UrlMultiAnalysisQueue : IUrlMultiAnalysisQueue
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysisQueue"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The provider for send endpoints.</param>
    public UrlMultiAnalysisQueue(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _endpointUriProvider = endpointUriProvider;
    }

    /// <inheritdoc/>
    public async Task QueueAsync(
        GlobalId multiAnalysisId,
        Uri url,
        GlobalId? correlationId,
        CancellationToken cancellationToken = default)
    {
        var jobMessage = new UrlAnalysisJobMessage(multiAnalysisId, url, correlationId);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeUrlUri);
        await sendEndpoint.Send(jobMessage, cancellationToken);
    }
}