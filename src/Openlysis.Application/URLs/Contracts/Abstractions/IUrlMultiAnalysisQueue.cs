using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.URLs.Contracts.Abstractions;

/// <summary>
/// Defines a service to queue URLs for multi analysis.
/// </summary>
public interface IUrlMultiAnalysisQueue
{
    /// <summary>
    /// Queues a URL to be analyzed.
    /// </summary>
    /// <param name="multiAnalysisId">The ID of the multi-analysis.</param>
    /// <param name="url">The URL to be analyzed.</param>
    /// <param name="correlationId">Optional correlation identifier that associates the analysis with a message analysis.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task QueueAsync(
        GlobalId multiAnalysisId,
        Uri url,
        GlobalId? correlationId,
        CancellationToken cancellationToken = default);
}