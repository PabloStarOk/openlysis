using System.Threading.Tasks;

using MassTransit;

using Openlysis.Infrastructure.Shared.Communication.Contracts;
using Openlysis.MultiAnalyzer.Abstractions;
using Openlysis.MultiAnalyzer.Models;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.Files;

/// <summary>
/// MassTransit consumer that handles <see cref="AnalyzeFile"/> messages.
/// </summary>
internal sealed class AnalyzeFileConsumer : IConsumer<AnalyzeFile>
{
    private readonly TimeoutRequestFactory<AnalyzeFile> _requestFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumer"/> class.
    /// </summary>
    /// <param name="requestFactory">
    /// The factory used to create <see cref="TimeoutRequest{AnalyzeFile}"/> instances.
    /// </param>
    public AnalyzeFileConsumer(TimeoutRequestFactory<AnalyzeFile> requestFactory)
    {
        _requestFactory = requestFactory;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<AnalyzeFile> context)
    {
        TimeoutRequest<AnalyzeFile> timeoutRequest = _requestFactory.Create();
        await timeoutRequest.ProcessAsync(context, context.CancellationToken);
        await _requestFactory.DisposeRequestAsync(timeoutRequest);
    }
}