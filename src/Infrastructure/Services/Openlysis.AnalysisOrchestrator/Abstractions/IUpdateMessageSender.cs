using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Openlysis.Domain.Common.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.AnalysisOrchestrator.Abstractions;

/// <summary>
/// Defines a contract for sending update messages for multi-analysis operations.
/// </summary>
internal interface IUpdateMessageSender
{
    /// <summary>
    /// Sends an update message for a multi-analysis operation.
    /// </summary>
    /// <typeparam name="TAnalysis">The type of analysis being updated.</typeparam>
    /// <param name="endpointProvider">The endpoint provider used to send the message.</param>
    /// <param name="message">The update message containing analysis data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public Task SendAsync<TAnalysis>(
        ISendEndpointProvider endpointProvider,
        UpdateMultiAnalysisMessage<TAnalysis> message,
        CancellationToken cancellationToken = default)
        where TAnalysis : Analysis;
}