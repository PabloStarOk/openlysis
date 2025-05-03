using MassTransit;

using Openlysis.Application.URLs.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Shared.Messaging.Abstractions;
using Openlysis.Infrastructure.Shared.Messaging.Models;

namespace Openlysis.Infrastructure.Services.URLs;

/// <summary>
/// Represents a service for analyzing URLs using multiple analyzers.
/// </summary>
internal class UrlMultiAnalyzer : IUrlMultiAnalyzer
{
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalyzer"/> class.
    /// </summary>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="sendEndpointProvider">The provider for send endpoints.</param>
    public UrlMultiAnalyzer(
        IEndpointUriProvider endpointUriProvider,
        ISendEndpointProvider sendEndpointProvider)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _endpointUriProvider = endpointUriProvider;
    }

    /// <inheritdoc/>
    public async Task StartAnalysisAsync(
        GlobalId multiAnalysisId,
        Uri url,
        CancellationToken cancellationToken = default)
    {
        var request = new AnalyzeUrl(multiAnalysisId, url);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeUrlUri);
        await sendEndpoint.Send(request, cancellationToken);
    }
}