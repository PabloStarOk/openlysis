using System.Text;

using Microsoft.IO;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.ValueObjects;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Entities;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Messages.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

namespace Openlysis.Infrastructure.Services;

/// <summary>
/// Builder for <see cref="MessageAnalysis"/>.
/// </summary>
internal sealed class MessageAnalysisBuilder : IMessageAnalysisBuilder
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;
    private readonly IHashService _hashService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisBuilder"/> class.
    /// </summary>
    /// <param name="memoryStreamManager">
    /// The manager for recyclable memory streams, used for efficient memory usage.
    /// </param>
    /// <param name="hashService">
    /// The service used to generate hashes for message content.
    /// </param>
    /// <param name="timeProvider">
    /// The provider for retrieving the current UTC time.
    /// </param>
    public MessageAnalysisBuilder(
        RecyclableMemoryStreamManager memoryStreamManager,
        IHashService hashService,
        TimeProvider timeProvider)
    {
        _memoryStreamManager = memoryStreamManager;
        _hashService = hashService;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<MessageAnalysis> BuildAsync(
        Message message,
        IEnumerable<FileMultiAnalysis> fileMultiAnalyses,
        IEnumerable<UrlMultiAnalysis> urlMultiAnalyses,
        IEnumerable<EmailAddressMultiReputation> emailAddressesReputations,
        IEnumerable<PhoneMultiReputation> phoneNumbersReputations,
        CancellationToken cancellationToken = default)
    {
        DataAssessmentResult<FileMetadata>[] attachedFileResults =
            CreateAttachedFilesResults(fileMultiAnalyses);

        DataAssessmentResult<Uri>[] urlResults =
            CreateUrlsResults(urlMultiAnalyses);

        DataAssessmentResult<string>[] emailResults =
            CreateEmailsResults(emailAddressesReputations);

        DataAssessmentResult<string>[] phoneResults =
            CreatePhonesResults(phoneNumbersReputations);

        var messageInfo = await CreateMessageInformationAsync(message, cancellationToken);

        // TODO: Add UserId and IsPrivate properties to domain model.
        return MessageAnalysis.Create(
            _timeProvider.GetUtcNow().UtcDateTime,
            messageInfo,
            Verdict.Unknown,
            attachedFileResults,
            urlResults,
            emailResults,
            phoneResults);
    }

    /// <summary>
    /// Creates an array of data assessment results for attached files
    /// based on the provided file multi-analyses.
    /// </summary>
    /// <param name="multiAnalyses">
    /// A collection of file multi-analyses containing metadata and IDs for the files.
    /// </param>
    /// <returns>
    /// An array of <see cref="DataAssessmentResult{FileMetadata}"/> representing the assessment results for the files.
    /// </returns>
    private static DataAssessmentResult<FileMetadata>[] CreateAttachedFilesResults(
        IEnumerable<FileMultiAnalysis> multiAnalyses)
    {
        return multiAnalyses
            .Select(
                f => DataAssessmentResult<FileMetadata>.Create(
                    DataType.File,
                    f.FileMetadata,
                    f.Id))
            .ToArray();
    }

    /// <summary>
    /// Creates an array of data assessment results for URLs
    /// based on the provided URL multi-analyses.
    /// </summary>
    /// <param name="multiAnalyses">
    /// A collection of URL multi-analyses containing URLs and their IDs.
    /// </param>
    /// <returns>
    /// An array of <see cref="DataAssessmentResult{Uri}"/> representing the assessment results for the URLs.
    /// </returns>
    private static DataAssessmentResult<Uri>[] CreateUrlsResults(
        IEnumerable<UrlMultiAnalysis> multiAnalyses)
    {
        return multiAnalyses
            .Select(u => DataAssessmentResult<Uri>.Create(
                DataType.Url,
                u.Url,
                u.Id))
            .ToArray();
    }

    /// <summary>
    /// Creates an array of data assessment results for email addresses
    /// based on the provided email address reputations.
    /// </summary>
    /// <param name="multiReputations">
    /// A collection of email address reputations containing email addresses and their IDs.
    /// </param>
    /// <returns>
    /// An array of <see cref="DataAssessmentResult{String}"/> representing the assessment results for the email addresses.
    /// </returns>
    private static DataAssessmentResult<string>[] CreateEmailsResults(
        IEnumerable<EmailAddressMultiReputation> multiReputations)
    {
        return multiReputations
            .Select(e => DataAssessmentResult<string>.Create(
                DataType.EmailAddress,
                e.EmailAddress,
                e.Id))
            .ToArray();
    }

    /// <summary>
    /// Creates an array of data assessment results for phone numbers
    /// based on the provided phone number reputations.
    /// </summary>
    /// <param name="multiReputations">
    /// A collection of phone number reputations containing phone information and their IDs.
    /// </param>
    /// <returns>
    /// An array of <see cref="DataAssessmentResult{String}"/> representing the assessment results for the phone numbers.
    /// </returns>
    private static DataAssessmentResult<string>[] CreatePhonesResults(
        IEnumerable<PhoneMultiReputation> multiReputations)
    {
        return multiReputations
            .Select(p => DataAssessmentResult<string>.Create(
                DataType.PhoneNumber,
                p.ServicesReputations[0].PhoneInfo.LocalFormat,
                p.Id))
            .ToArray();
    }

    /// <summary>
    /// Generates a hash set for the given message by combining its sender, subject, and content.
    /// </summary>
    /// <param name="message">The message to hash, containing sender, subject, and content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated <see cref="ContentHashSet"/>.</returns>
    private async Task<ContentHashSet> GenerateHashAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        string mergedMessage = string.Join(
            string.Empty,
            message.Sender,
            message.Subject,
            message.Content);
        byte[] messageBytes = Encoding.UTF8.GetBytes(mergedMessage);
        await using var memoryStream = _memoryStreamManager.GetStream(
            nameof(GenerateHashAsync),
            messageBytes);
        return await _hashService.HashDataAsync(memoryStream, cancellationToken);
    }

    /// <summary>
    /// Creates a <see cref="MessageInformation"/> object for the given message.
    /// </summary>
    /// <param name="message">The message containing details such as type, sender, subject, and content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the created <see cref="MessageInformation"/>.
    /// </returns>
    private async Task<MessageInformation> CreateMessageInformationAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        ContentHashSet messageHashSet
            = await GenerateHashAsync(message, cancellationToken);

        return new MessageInformation(
            message.Type,
            message.Sender,
            message.Subject,
            message.Content,
            messageHashSet);
    }
}