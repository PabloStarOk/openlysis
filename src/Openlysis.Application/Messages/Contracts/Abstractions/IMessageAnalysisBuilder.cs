using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.Messages.Contracts.Abstractions;

/// <summary>
/// Defines a builder of <see cref="MessageAnalysis"/>.
/// </summary>
public interface IMessageAnalysisBuilder
{
    /// <summary>
    /// Builds a new <see cref="MessageAnalysis"/> instance based on the provided message and its associated analyses.
    /// </summary>
    /// <param name="userId">The unique identifier of the user associated with the message analysis.</param>
    /// <param name="isPrivate">Indicates whether the message is private.</param>
    /// <param name="message">The message to analyze.</param>
    /// <param name="fileMultiAnalyses">A collection of file analyses associated with the message.</param>
    /// <param name="urlMultiAnalyses">A collection of URL analyses associated with the message.</param>
    /// <param name="emailAddressesReputations">A collection of email address reputations associated with the message.</param>
    /// <param name="phoneNumbersReputations">A collection of phone number reputations associated with the message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the resulting <see cref="MessageAnalysis"/>.</returns>
    public Task<MessageAnalysis> BuildAsync(
        UserId userId,
        bool isPrivate,
        Message message,
        IEnumerable<FileMultiAnalysis> fileMultiAnalyses,
        IEnumerable<UrlMultiAnalysis> urlMultiAnalyses,
        IEnumerable<EmailAddressMultiReputation> emailAddressesReputations,
        IEnumerable<PhoneMultiReputation> phoneNumbersReputations,
        CancellationToken cancellationToken = default);
}