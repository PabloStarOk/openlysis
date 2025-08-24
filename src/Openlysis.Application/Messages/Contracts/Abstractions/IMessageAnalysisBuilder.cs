using Openlysis.Application.Common.Models;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

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
        GlobalId userId,
        bool isPrivate);

    /// <summary>
    /// Adds message information and associated file data to the analysis.
    /// </summary>
    /// <param name="message">The message to be analyzed.</param>
    /// <param name="files">An array of <see cref="ProcessedFile"/> representing the files attached to the message.</param>
    /// <returns>The current instance of <see cref="IMessageAnalysisBuilder"/>.</returns>
    public IMessageAnalysisBuilder WithMessageInformation(
        Message message,
        ProcessedFile[] files);

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
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the resulting <see cref="HashValues"/>.
    /// </returns>
    public ValueTask<HashValues> GenerateHashAsync(
        CancellationToken cancellationToken = default);
}