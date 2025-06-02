using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

namespace Openlysis.MultiAnalyzer.Abstractions;

/// <summary>
/// Defines an orchestrator for handling requests of type <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request to orchestrate.</typeparam>
internal interface IRequestOrchestrator<in TRequest>
    where TRequest : class
{
    /// <summary>
    /// Executes the orchestration logic for the given request context.
    /// </summary>
    /// <param name="context">The MassTransit consume context for the request.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExecuteAsync(
        ConsumeContext<TRequest> context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Invoked when the request times out.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous timeout handling operation.</returns>
    public Task OnTimeoutAsync();
}
