using System;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;

namespace Openlysis.MultiAnalyzer.Infrastructure.Communication;

/// <summary>
/// Sends update messages for multi-analysis operations involving <see cref="UrlAnalysis"/> and <see cref="FileAnalysis"/>.
/// Implements <see cref="IUpdateMessageSender{T}"/> twice to support both types.
/// </summary>
internal sealed class UpdateMultiAnalysisMessageSender
    : IUpdateMessageSender<UpdateMultiAnalysisMessage<UrlAnalysis>>,
    IUpdateMessageSender<UpdateMultiAnalysisMessage<FileAnalysis>>
{
    private readonly IEndpointUriProvider _brokerEpProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMultiAnalysisMessageSender"/> class.
    /// </summary>
    /// <param name="brokerEpProvider">
    /// The provider for endpoint URIs used to send update messages.
    /// </param>
    public UpdateMultiAnalysisMessageSender(IEndpointUriProvider brokerEpProvider)
    {
        _brokerEpProvider = brokerEpProvider;
    }

    /// <inheritdoc/>
    public async Task SendAsync(
        ConsumeContext context,
        UpdateMultiAnalysisMessage<UrlAnalysis> message,
        CancellationToken cancellationToken = default)
    {
        Uri uri = _brokerEpProvider.UpdateUrlMultiAnalysisUri;
        ISendEndpoint endpoint = await context.GetSendEndpoint(uri);
        await endpoint.Send(message, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendAsync(
        ConsumeContext context,
        UpdateMultiAnalysisMessage<FileAnalysis> message,
        CancellationToken cancellationToken = default)
    {
        Uri uri = _brokerEpProvider.UpdateFileMultiAnalysisUri;
        ISendEndpoint endpoint = await context.GetSendEndpoint(uri);
        await endpoint.Send(message, cancellationToken);
    }
}