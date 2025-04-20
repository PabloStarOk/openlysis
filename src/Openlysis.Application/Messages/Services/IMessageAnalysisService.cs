using ErrorOr;

using Openlysis.Application.Common.Enums;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.Messages.Services;

/// <summary>
/// Defines an analysis service for messages.
/// </summary>
public interface IMessageAnalysisService
{
    /// <summary>
    /// Analyzes a message asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user performing the analysis.</param>
    /// <param name="isPrivate">Indicates whether the message analysis is only available for the user who sends the request.</param>
    /// <param name="message">The message to be analyzed.</param>
    /// <param name="files">An optional array of files associated with the message.</param>
    /// <param name="reanalyzeData">Indicates whether to reanalyze the data even if it has been analyzed before.</param>
    /// <param name="requestCountryCode">The country code of the request origin, if available.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AnalyzeAsync(
        UserId userId,
        bool isPrivate,
        Message message,
        FileData[]? files,
        bool reanalyzeData,
        string? requestCountryCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a message analysis by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique global identifier of the message analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing an <see cref="ErrorOr{T}"/>
    /// with the result of the message analysis or an error.
    /// </returns>
    public Task<ErrorOr<MessageAnalysis>> GetAnalysisByIdAsync(
        GlobalId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of message analyses by their hash asynchronously.
    /// </summary>
    /// <param name="hash">The hash value used to identify the message analyses.</param>
    /// <param name="amount">The maximum number of analyses to retrieve.</param>
    /// <param name="order">The order in which the analyses should be retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a read-only list of <see cref="MessageAnalysis"/>.
    /// </returns>
    public Task<IReadOnlyList<MessageAnalysis>> GetAnalysesByHashAsync(
        string hash,
        int amount,
        OrderType order,
        CancellationToken cancellationToken = default);
}