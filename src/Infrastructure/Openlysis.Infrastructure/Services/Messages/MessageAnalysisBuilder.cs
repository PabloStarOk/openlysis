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
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Infrastructure.Services.Messages;

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
        UserId userId,
        bool isPrivate,
        Message message,
        Stream[] filesData,
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

        var messageInfo = await CreateMessageInformationAsync(
            message,
            filesData,
            cancellationToken);

        return MessageAnalysis.Create(
            _timeProvider.GetUtcNow().UtcDateTime,
            userId,
            isPrivate,
            messageInfo,
            Verdict.Unknown,
            attachedFileResults,
            urlResults,
            emailResults,
            phoneResults);
    }

    /// <inheritdoc/>
    public async Task<ContentHashSet> GenerateHashAsync(
        Message message,
        Stream[] filesData,
        CancellationToken cancellationToken = default)
    {
        ContentHashSet messageHashValues = await HashMessageAsync(
            message,
            cancellationToken);

        if (filesData.Length is 0)
        {
            return messageHashValues;
        }

        ContentHashSet[] filesHashValues = await HashFilesAsync(
            filesData,
            cancellationToken);

        string compositeHash = CreateCompositeHash(
            messageHashValues,
            filesHashValues);

        return await HashStringAsync(compositeHash, cancellationToken);
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
    /// Creates a composite hash by combining the SHA-256 hash of the message
    /// with the SHA-256 hashes of the associated files.
    /// </summary>
    /// <param name="messageHashValues">
    /// The hash values of the message content.
    /// </param>
    /// <param name="filesHashValues">
    /// The hash values of the associated files.
    /// </param>
    /// <returns>
    /// A string representing the composite hash.
    /// </returns>
    private static string CreateCompositeHash(
        ContentHashSet messageHashValues,
        params ContentHashSet[] filesHashValues)
    {
        var filesSha256Values = filesHashValues.Select(f => f.Sha256);
        string concatenatedValues = string
            .Concat(filesSha256Values.OrderDescending());
        return string.Concat(messageHashValues.Sha256, concatenatedValues);
    }

    /// <summary>
    /// Computes the SHA-256 hash of the given message.
    /// </summary>
    /// <param name="message">The message containing metadata to be hashed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the SHA-256 hash string of the message.
    /// </returns>
    private async Task<ContentHashSet> HashMessageAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        string messageMetadata = string.Join(
            separator: string.Empty,
            message.Sender,
            message.Subject,
            message.Content);

        return await HashStringAsync(
            messageMetadata,
            cancellationToken);
    }

    /// <summary>
    /// Computes the SHA-256 hash values for the provided file data streams.
    /// </summary>
    /// <param name="filesData">An array of streams representing the file data to be hashed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing an array of <see cref="ContentHashSet"/> 
    /// with the computed hash values for each file.
    /// </returns>
    private async Task<ContentHashSet[]> HashFilesAsync(
        Stream[] filesData,
        CancellationToken cancellationToken = default)
    {
        List<ContentHashSet> filesHashValues = [];
        foreach (var data in filesData)
        {
            ContentHashSet hashValues = await _hashService.HashDataAsync(data, cancellationToken);
            filesHashValues.Add(hashValues);
        }

        return filesHashValues.ToArray();
    }

    /// <summary>
    /// Computes a hash from the given string using the SHA-256 algorithm.
    /// </summary>
    /// <param name="input">The input string to be hashed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the computed <see cref="ContentHashSet"/>.
    /// </returns>
    private async Task<ContentHashSet> HashStringAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        await using var memoryStream = _memoryStreamManager.GetStream(
            nameof(HashStringAsync),
            inputBytes);

        return await _hashService.HashDataAsync(memoryStream, cancellationToken);
    }

    /// <summary>
    /// Creates a <see cref="MessageInformation"/> object for the given message and associated file data.
    /// </summary>
    /// <param name="message">The message containing details such as type, sender, subject, and content.</param>
    /// <param name="filesData">An array of streams representing the file data associated with the message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the created <see cref="MessageInformation"/>.
    /// </returns>
    private async Task<MessageInformation> CreateMessageInformationAsync(
        Message message,
        Stream[] filesData,
        CancellationToken cancellationToken)
    {
        ContentHashSet messageHashSet = await GenerateHashAsync(
            message,
            filesData,
            cancellationToken);

        return new MessageInformation(
            message.Type,
            message.Sender,
            message.Subject,
            message.Content,
            messageHashSet);
    }
}