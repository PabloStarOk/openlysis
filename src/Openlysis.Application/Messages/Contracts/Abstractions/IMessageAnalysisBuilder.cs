using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;
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
    /// Sets the user context for the message analysis.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <returns>The current instance of <see cref="IMessageAnalysisBuilder"/>.</returns>
    public IMessageAnalysisBuilder WithUserContext(
        UserId userId,
        bool isPrivate);

    /// <summary>
    /// Adds message information and associated file data to the analysis.
    /// </summary>
    /// <param name="message">The message to be analyzed.</param>
    /// <param name="filesData">An array of streams representing the file data.</param>
    /// <returns>The current instance of <see cref="IMessageAnalysisBuilder"/>.</returns>
    public IMessageAnalysisBuilder WithMessageInformation(
        Message message,
        Stream[] filesData);

    /// <summary>
    /// Adds file multi-analyses to the message analysis.
    /// </summary>
    /// <param name="multiAnalyses">A collection of file multi-analyses.</param>
    /// <returns>The current instance of <see cref="IMessageAnalysisBuilder"/>.</returns>
    public IMessageAnalysisBuilder WithFileMultiAnalyses(
        IEnumerable<FileMultiAnalysis> multiAnalyses);

    /// <summary>
    /// Adds URL multi-analyses to the message analysis.
    /// </summary>
    /// <param name="multiAnalyses">A collection of URL multi-analyses.</param>
    /// <returns>The current instance of <see cref="IMessageAnalysisBuilder"/>.</returns>
    public IMessageAnalysisBuilder WithUrlMultiAnalyses(
        IEnumerable<UrlMultiAnalysis> multiAnalyses);

    /// <summary>
    /// Adds email address multi-reputations to the message analysis.
    /// </summary>
    /// <param name="multiReputations">A collection of email address multi-reputations.</param>
    /// <returns>The current instance of <see cref="IMessageAnalysisBuilder"/>.</returns>
    public IMessageAnalysisBuilder WithEmailAddressMultiReputations(
        IEnumerable<EmailAddressMultiReputation> multiReputations);

    /// <summary>
    /// Adds phone number multi-reputations to the message analysis.
    /// </summary>
    /// <param name="multiReputations">A collection of phone number multi-reputations.</param>
    /// <returns>The current instance of <see cref="IMessageAnalysisBuilder"/>.</returns>
    public IMessageAnalysisBuilder WithPhoneNumberMultiReputations(
        IEnumerable<PhoneMultiReputation> multiReputations);

    /// <summary>
    /// Builds the message analysis asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the resulting <see cref="MessageAnalysis"/>.</returns>
    public Task<MessageAnalysis> BuildAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a hash set of content based on the provided message.
    /// This hash set is used to uniquely identify the message content
    /// by combining its sender, subject, and content into a single hash.
    /// </summary>
    /// <param name="message">The message for which the content hash set will be generated.</param>
    /// <param name="filesData">An array of streams representing the file data to be included in the hash generation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the resulting <see cref="ContentHashSet"/>.
    /// </returns>
    public Task<ContentHashSet> GenerateHashAsync(
        Message message,
        Stream[] filesData,
        CancellationToken cancellationToken = default);
}