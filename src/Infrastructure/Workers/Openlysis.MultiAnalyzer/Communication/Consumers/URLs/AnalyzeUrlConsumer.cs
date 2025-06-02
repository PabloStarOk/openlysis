using System.Threading.Tasks;

using MassTransit;

using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Models;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.URLs;

/// <summary>
/// MassTransit consumer that handles <see cref="AnalyzeUrl"/> messages.
/// </summary>
internal sealed class AnalyzeUrlConsumer : IConsumer<AnalyzeUrl>
{
    private readonly TimeoutRequestFactory<AnalyzeUrl> _requestFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlConsumer"/> class.
    /// </summary>
    /// <param name="requestFactory">
    /// The factory used to create <see cref="TimeoutRequest{AnalyzeUrl}"/> instances.
    /// </param>
    public AnalyzeUrlConsumer(TimeoutRequestFactory<AnalyzeUrl> requestFactory)
    {
        _requestFactory = requestFactory;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<AnalyzeUrl> context)
    {
        TimeoutRequest<AnalyzeUrl> timeoutRequest = _requestFactory.Create();
        await timeoutRequest.ProcessAsync(context, context.CancellationToken);
        await _requestFactory.DisposeRequestAsync(timeoutRequest);
    }
}