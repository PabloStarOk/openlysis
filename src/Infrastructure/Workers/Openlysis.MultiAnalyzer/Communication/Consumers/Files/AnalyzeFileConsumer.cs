using System.Threading.Tasks;

using MassTransit;

using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Models;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.Files;

/// <summary>
/// MassTransit consumer that handles <see cref="AnalyzeFileMessage"/> messages.
/// </summary>
internal sealed class AnalyzeFileConsumer : IConsumer<AnalyzeFileMessage>
{
    private readonly TimeoutRequestFactory<AnalyzeFileMessage> _requestFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumer"/> class.
    /// </summary>
    /// <param name="requestFactory">
    /// The factory used to create <see cref="TimeoutRequest{AnalyzeFile}"/> instances.
    /// </param>
    public AnalyzeFileConsumer(TimeoutRequestFactory<AnalyzeFileMessage> requestFactory)
    {
        _requestFactory = requestFactory;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<AnalyzeFileMessage> context)
    {
        TimeoutRequest<AnalyzeFileMessage> timeoutRequest = _requestFactory.Create();
        await timeoutRequest.ProcessAsync(context, context.CancellationToken);
        await _requestFactory.DisposeRequestAsync(timeoutRequest);
    }
}