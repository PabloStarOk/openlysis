using System;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;

namespace Openlysis.MultiAnalyzer.Infrastructure;

/// <summary>
/// Sends update messages for multi-analysis operations involving <see cref="UrlAnalysis"/> and <see cref="FileAnalysis"/>.
/// </summary>
internal sealed class UpdateMultiAnalysisMessageSender : IUpdateMessageSender
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
    public async Task SendAsync<TAnalysis>(
        ISendEndpointProvider endpointProvider,
        UpdateMultiAnalysisMessage<TAnalysis> message,
        CancellationToken cancellationToken = default)
        where TAnalysis : Analysis
    {
        Uri endpointUri = message switch
        {
            UpdateMultiAnalysisMessage<FileAnalysis> => _brokerEpProvider.UpdateFileMultiAnalysisUri,
            UpdateMultiAnalysisMessage<UrlAnalysis> => _brokerEpProvider.UpdateUrlMultiAnalysisUri,
            _ => throw new ArgumentOutOfRangeException(nameof(message), message, "Unsupported analysis type for multi-analysis update."),
        };

        ISendEndpoint endpoint = await endpointProvider.GetSendEndpoint(endpointUri);
        await endpoint.Send(message, cancellationToken);
    }
}