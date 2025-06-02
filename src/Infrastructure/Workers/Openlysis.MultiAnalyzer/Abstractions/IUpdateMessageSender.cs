using System.Threading;
using System.Threading.Tasks;

using MassTransit;

namespace Openlysis.MultiAnalyzer.Abstractions;

/// <summary>
/// Defines a contract for sending update messages asynchronously.
/// </summary>
/// <typeparam name="TMessage">The type of the message to send.</typeparam>
internal interface IUpdateMessageSender<in TMessage>
    where TMessage : class
{
    /// <summary>
    /// Sends an update message asynchronously using the provided MassTransit consume context.
    /// </summary>
    /// <param name="context">The MassTransit consume context for the current message.</param>
    /// <param name="message">The update message to send.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendAsync(
        ConsumeContext context,
        TMessage message,
        CancellationToken cancellationToken = default);
}