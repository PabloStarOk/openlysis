using System;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.MultiAnalyzer.Core.Abstractions;
using Openlysis.MultiAnalyzer.Features.URLs.Analyze.Contracts;

namespace Openlysis.MultiAnalyzer.Application.Services;

/// <summary>
/// Represents a service for analyzing URLs using multiple analyzers.
/// </summary>
public class UrlMultiAnalyzer : IUrlMultiAnalyzer
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
        MultiAnalysisId multiAnalysisId,
        Uri url,
        CancellationToken cancellationToken = default)
    {
        var request = new AnalyzeUrl(multiAnalysisId, url);

        ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(_endpointUriProvider.AnalyzeUrlUri);
        await sendEndpoint.Send(request, cancellationToken);
    }
}