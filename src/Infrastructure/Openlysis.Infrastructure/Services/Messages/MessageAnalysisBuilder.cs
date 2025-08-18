using System.Collections.Concurrent;
using System.Net.Mail;
using System.Text;

using Microsoft.IO;

using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Entities;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Messages.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// Builder for <see cref="MessageAnalysis"/>.
/// </summary>
internal sealed class MessageAnalysisBuilder : IMessageAnalysisBuilder
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;
    private readonly IHashService _hashService;
    private readonly TimeProvider _timeProvider;
    private BuildState _buildState = new ();

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
    public IMessageAnalysisBuilder WithUserContext(
        GlobalId userId, bool isPrivate)
    {
        _buildState = _buildState with
        {
            UserId = userId,
            IsPrivate = isPrivate
        };

        return this;
    }

    /// <inheritdoc/>
    public IMessageAnalysisBuilder WithMessageInformation(
        Message message,
        Stream[] filesData)
    {
        _buildState = _buildState with
        {
            Message = message,
            FilesData = filesData
        };
        return this;
    }

    /// <inheritdoc/>
    public IMessageAnalysisBuilder WithFileMultiAnalyses(
        IEnumerable<FileMultiAnalysis> multiAnalyses)
    {
        DataAssessmentResult<string>[] attachedFileResults =
            CreateAttachedFilesResults(multiAnalyses);

        _buildState = _buildState with
        {
            FileResults = attachedFileResults,
        };
        return this;
    }

    /// <inheritdoc/>
    public IMessageAnalysisBuilder WithUrlMultiAnalyses(
        IEnumerable<UrlMultiAnalysis> multiAnalyses)
    {
        DataAssessmentResult<Uri>[] urlResults =
            CreateUrlsResults(multiAnalyses);

        _buildState = _buildState with
        {
            UrlResults = urlResults,
        };
        return this;
    }

    /// <inheritdoc/>
    public IMessageAnalysisBuilder WithEmailAddressMultiReputations(
        IEnumerable<EmailAddressMultiReputation> multiReputations)
    {
        DataAssessmentResult<MailAddress>[] emailResults =
            CreateEmailsResults(multiReputations);

        _buildState = _buildState with
        {
            EmailResults = emailResults,
        };
        return this;
    }

    /// <inheritdoc/>
    public IMessageAnalysisBuilder WithPhoneNumberMultiReputations(
        IEnumerable<PhoneMultiReputation> multiReputations)
    {
        DataAssessmentResult<string>[] phoneResults =
            CreatePhonesResults(multiReputations);

        _buildState = _buildState with
        {
            PhoneResults = phoneResults,
        };
        return this;
    }

    /// <inheritdoc/>
    public async Task<MessageAnalysis> BuildAsync(
        CancellationToken cancellationToken = default)
    {
        if (_buildState.UserId is null
            || _buildState.IsPrivate is null
            || _buildState.Message is null
            || _buildState.FilesData is null
            || _buildState.FileResults is null
            || _buildState.UrlResults is null
            || _buildState.EmailResults is null
            || _buildState.PhoneResults is null)
        {
            throw new InvalidOperationException("Cannot build message analysis. One or more required properties in the build state are null.");
        }

        var messageInfo = await CreateMessageInformationAsync(
            _buildState.Message,
            cancellationToken);

        return MessageAnalysis.Create(
            _timeProvider.GetUtcNow().UtcDateTime,
            _buildState.UserId,
            (bool)_buildState.IsPrivate,
            messageInfo,
            Verdict.Unknown,
            _buildState.FileResults,
            _buildState.UrlResults,
            _buildState.EmailResults,
            _buildState.PhoneResults);
    }

    /// <inheritdoc/>
    public async ValueTask<HashValues> GenerateHashAsync(
        CancellationToken cancellationToken = default)
    {
        if (_buildState.Message is null
            || _buildState.FilesData is null)
        {
            throw new InvalidOperationException("Message information and associated files must be set before generating the hash of the message.");
        }

        if (_buildState.HashValues is not null)
        {
            return _buildState.HashValues;
        }

        HashValues messageHashValues = await HashMessageAsync(
            _buildState.Message,
            cancellationToken);

        if (_buildState.FilesData.Length is 0)
        {
            _buildState = _buildState with
            {
                HashValues = messageHashValues
            };
            return messageHashValues;
        }

        HashValues[] filesHashValues = await HashFilesAsync(
            _buildState.FilesData,
            cancellationToken);

        string compositeHash = CreateCompositeHash(
            messageHashValues,
            filesHashValues);

        HashValues compositeHashValues = await HashStringAsync(
            compositeHash,
            cancellationToken);

        _buildState = _buildState with
        {
            HashValues = compositeHashValues
        };
        return _buildState.HashValues;
    }

    /// <summary>
    /// Represents the state of the builder during the construction of a `MessageAnalysis`.
    /// </summary>
    /// <param name="UserId">The ID of the user associated with the message analysis.</param>
    /// <param name="IsPrivate">Indicates whether the message is private.</param>
    /// <param name="Message">The message being analyzed.</param>
    /// <param name="FilesData">The data streams of the files attached to the message.</param>
    /// <param name="HashValues">The hash values of the message and associated files.</param>
    /// <param name="FileResults">The assessment results for the attached files.</param>
    /// <param name="UrlResults">The assessment results for the URLs in the message.</param>
    /// <param name="EmailResults">The assessment results for the email addresses in the message.</param>
    /// <param name="PhoneResults">The assessment results for the phone numbers in the message.</param>
    /// <remarks>
    /// This record is used to store the intermediate state of the builder, including user context,
    /// message details, and analysis results for files, URLs, email addresses, and phone numbers.
    /// </remarks>
    private sealed record BuildState(
        GlobalId? UserId = null,
        bool? IsPrivate = null,
        Message? Message = null,
        Stream[]? FilesData = null,
        HashValues? HashValues = null,
        DataAssessmentResult<string>[]? FileResults = null,
        DataAssessmentResult<Uri>[]? UrlResults = null,
        DataAssessmentResult<MailAddress>[]? EmailResults = null,
        DataAssessmentResult<string>[]? PhoneResults = null);

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
    private static DataAssessmentResult<string>[] CreateAttachedFilesResults(
        IEnumerable<FileMultiAnalysis> multiAnalyses)
    {
        return multiAnalyses
            .Select(
                f => DataAssessmentResult<string>.Create(
                    DataType.File,
                    f.FileMetadata.Name,
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
    private static DataAssessmentResult<MailAddress>[] CreateEmailsResults(
        IEnumerable<EmailAddressMultiReputation> multiReputations)
    {
        return multiReputations
            .Select(e => DataAssessmentResult<MailAddress>.Create(
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
                p.Reputations[0].PhoneInfo.LocalFormat,
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
        HashValues messageHashValues,
        params HashValues[] filesHashValues)
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
    private async Task<HashValues> HashMessageAsync(
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
    /// A task that represents the asynchronous operation, containing an array of <see cref="HashValues"/>
    /// with the computed hash values for each file.
    /// </returns>
    private async Task<HashValues[]> HashFilesAsync(
        Stream[] filesData,
        CancellationToken cancellationToken = default)
    {
        ConcurrentBag<HashValues> filesHashValues = [];
        await Parallel.ForEachAsync(
            filesData,
            cancellationToken,
            async (data, ct) =>
            {
                HashValues hashValues = await _hashService.HashDataAsync(
                    data,
                    ct);

                filesHashValues.Add(hashValues);
            });

        return filesHashValues.ToArray();
    }

    /// <summary>
    /// Computes a hash from the given string using the SHA-256 algorithm.
    /// </summary>
    /// <param name="input">The input string to be hashed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the computed <see cref="HashValues"/>.
    /// </returns>
    private async Task<HashValues> HashStringAsync(
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
        HashValues messageHashValues = await GenerateHashAsync(cancellationToken);

        return new MessageInformation(
            message.Type,
            message.Sender,
            message.Subject,
            message.Content,
            messageHashValues);
    }
}