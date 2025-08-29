using Openlysis.Application.Common.Models;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;

namespace Openlysis.Application.Messages.Contracts.Abstractions;

/// <summary>
/// Provides functionality to generate hash values for messages and their associated processed files.
/// </summary>
public interface IMessageHashService
{
    /// <summary>
    /// Asynchronously computes hash values for the specified message and its processed files.
    /// </summary>
    /// <param name="message">The message to hash.</param>
    /// <param name="processedFiles">A collection of processed files related to the message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{HashValues}"/> representing the asynchronous operation, containing the computed hash values.</returns>
    public ValueTask<HashValues> HashAsync(
        Message message,
        IEnumerable<ProcessedFile> processedFiles,
        CancellationToken cancellationToken = default);
}