using ErrorOr;

using Openlysis.Application.Common.Enums;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.Messages.Services;

/// <summary>
/// Defines an analysis service for messages.
/// </summary>
public interface IMessageAnalysisService
{
    /// <summary>
    /// Gets a value indicating whether the analysis service is available.
    /// </summary>
    public bool AnalyzeIsAvailable { get; }

    /// <summary>
    /// Analyzes a message asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user performing the analysis.</param>
    /// <param name="isPrivate">Indicates whether the message analysis is private to the requesting user.</param>
    /// <param name="message">The message to analyze.</param>
    /// <param name="files">An optional array of files associated with the message for analysis.</param>
    /// <param name="reanalyze">Specifies whether to reanalyze the message even if there is an existing analysis available to retrieve.</param>
    /// <param name="requestCountryCode">The country code of the request origin, if provided.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the message analysis.</returns>
    public Task<ErrorOr<MessageAnalysis>> AnalyzeAsync(
        UserId userId,
        bool isPrivate,
        Message message,
        FileData[]? files,
        bool reanalyze,
        string? requestCountryCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a message analysis by its unique identifier asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the analysis.</param>
    /// <param name="id">The unique global identifier of the message analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing an <see cref="ErrorOr{T}"/>
    /// with the result of the message analysis or an error.
    /// </returns>
    public Task<ErrorOr<MessageAnalysis>> GetAnalysisByIdAsync(
        UserId userId,
        GlobalId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of message analyses by their hash asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the analyses.</param>
    /// <param name="hash">The hash value used to identify the message analyses.</param>
    /// <param name="page">The page number for pagination (starting from 1).</param>
    /// <param name="pageSize">The number of analyses to retrieve per page.</param>
    /// <param name="order">The order in which the analyses should be retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a read-only list of <see cref="MessageAnalysis"/>.
    /// </returns>
    public Task<IReadOnlyList<MessageAnalysis>> GetAnalysesByHashAsync(
        UserId userId,
        string hash,
        int page,
        int pageSize,
        OrderType order,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of message analyses performed by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose message analyses are to be retrieved.</param>
    /// <param name="messageType">The type of message to filter the analyses by.</param>
    /// <param name="page">The page number of the results to retrieve.</param>
    /// <param name="pageSize">The number of message analyses per page.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of <see cref="MessageAnalysis"/> objects for the specified user.</returns>
    public Task<IReadOnlyList<MessageAnalysis>> GetAnalysesByUserAsync(
        UserId userId,
        MessageType messageType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}