using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

using ErrorOr;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Enums;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.Messages.Services;

/// <summary>
/// Service for analyzing a messages and retrieving the analyses.
/// </summary>
internal class MessageAnalysisService : IMessageAnalysisService
{
    private readonly IRepository<MessageAnalysis, GlobalId> _repository;
    private readonly IMessageDataExtractor _dataExtractor;
    private readonly IMessageAnalyzer _messageAnalyzer;
    private readonly IMessageAnalysisBuilder _messageAnalysisBuilder;
    private readonly IMessageAnalysisUpdater _messageAnalysisUpdater;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisService"/> class.
    /// </summary>
    /// <param name="repository">The repository for managing <see cref="MessageAnalysis"/> entities.</param>
    /// <param name="dataExtractor">The service responsible for extracting data (e.g., URLs, email addresses, phone numbers) from messages.</param>
    /// <param name="messageAnalyzer">The service responsible for analyzing messages and their associated data.</param>
    /// <param name="messageAnalysisBuilder">The service responsible for building message analysis objects.</param>
    /// <param name="messageAnalysisUpdater">The service responsible for updating message analysis data.</param>
    public MessageAnalysisService(
        IRepository<MessageAnalysis, GlobalId> repository,
        IMessageDataExtractor dataExtractor,
        IMessageAnalyzer messageAnalyzer,
        IMessageAnalysisBuilder messageAnalysisBuilder,
        IMessageAnalysisUpdater messageAnalysisUpdater)
    {
        _repository = repository;
        _dataExtractor = dataExtractor;
        _messageAnalyzer = messageAnalyzer;
        _messageAnalysisBuilder = messageAnalysisBuilder;
        _messageAnalysisUpdater = messageAnalysisUpdater;
    }

    /// <inheritdoc/>
    public async Task<MessageAnalysis> AnalyzeAsync(
        UserId userId,
        bool isPrivate,
        Message message,
        FileData[]? files,
        bool reanalyzeData,
        string? requestCountryCode,
        CancellationToken cancellationToken = default)
    {
        Stream[] filesData = GetFileDataStreams(files);
        MessageAnalysis? lastExistingAnalysis = await FetchLastAnalysisAsync(
                message,
                filesData,
                cancellationToken);

        if (lastExistingAnalysis is not null
            && !reanalyzeData)
        {
            return lastExistingAnalysis;
        }

        string? subject = message.Subject;
        string content = message.Content;

        if (requestCountryCode is not null)
        {
            _dataExtractor.SetRequestCountryCode(requestCountryCode);
        }

        IEnumerable<Uri> urls =
            ExtractData(subject, content, _dataExtractor.ExtractUrls);
        HashSet<MailAddress> emailAddresses =
            ExtractData(subject, content, _dataExtractor.ExtractEmailAddresses)
                .ToHashSet();
        IEnumerable<string> phoneNumbers =
            ExtractData(subject, content, _dataExtractor.ExtractPhoneNumbers);
        if (TryCreateSenderEmail(message, out MailAddress? senderEmail))
        {
            emailAddresses.Add(senderEmail);
        }

        IEnumerable<FileMultiAnalysis> fileMultiAnalyses = [];
        if (files is not null)
        {
            fileMultiAnalyses = await _messageAnalyzer
                .AnalyzeFilesAsync(userId, isPrivate, reanalyzeData, files, cancellationToken);
        }

        IEnumerable<UrlMultiAnalysis> urlMultiAnalyses =
            await _messageAnalyzer.AnalyzeUrlsAsync(userId, isPrivate, urls, cancellationToken);
        IEnumerable<EmailAddressMultiReputation> emailAddressesReputations =
            await _messageAnalyzer.GetEmailAddressesReputationsAsync(emailAddresses, cancellationToken);
        IEnumerable<PhoneMultiReputation> phoneNumbersReputations =
            await _messageAnalyzer.GetPhoneNumbersReputationsAsync(phoneNumbers, cancellationToken);

        MessageAnalysis messageAnalysis = await _messageAnalysisBuilder.BuildAsync(
            userId,
            isPrivate,
            message,
            filesData,
            fileMultiAnalyses,
            urlMultiAnalyses,
            emailAddressesReputations,
            phoneNumbersReputations,
            cancellationToken);

        await _messageAnalysisUpdater.AddPendingAsync(
            messageAnalysis,
            cancellationToken);

        return messageAnalysis;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<MessageAnalysis>> GetAnalysisByIdAsync(
        UserId userId,
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        MessageAnalysis? messageAnalysis = await _repository.GetAsync(id, cancellationToken);
        if (messageAnalysis is null)
        {
            return Error.NotFound();
        }

        if (messageAnalysis.IsPrivate &&
            messageAnalysis.UserId != userId)
        {
            return Error.NotFound();
        }

        return messageAnalysis;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MessageAnalysis>> GetAnalysesByHashAsync(
        UserId userId,
        string hash,
        int amount,
        OrderType order,
        CancellationToken cancellationToken = default)
    {
        var analyses = await _repository.GetManyAsync(
            amount,
            u => (u.Message.MessageHashSet.Sha256 == hash
                    || u.Message.MessageHashSet.Md5 == hash
                    || u.Message.MessageHashSet.Sha1 == hash
                    || u.Message.MessageHashSet.Sha512 == hash)
                && (!u.IsPrivate || (u.IsPrivate && u.UserId == userId)),
            OrderBy,
            cancellationToken);

        return analyses.ToList().AsReadOnly();

        // Order
        IOrderedQueryable<MessageAnalysis> OrderBy(IQueryable<MessageAnalysis> q)
        {
            return order switch
            {
                OrderType.Dsc => q.OrderByDescending(u => u.StartedDate),
                OrderType.Asc => q.OrderBy(u => u.StartedDate),
                _ => throw new InvalidOperationException("StartedDateOrder has an invalid enum value.")
            };
        }
    }

    /// <summary>
    /// Extracts data from the provided subject and content using the specified extraction method.
    /// </summary>
    /// <typeparam name="TData">The type of data to extract.</typeparam>
    /// <param name="subject">The subject of the message, which may contain data to extract.</param>
    /// <param name="content">The content of the message, which must contain data to extract.</param>
    /// <param name="extractionMethod">The method used to extract data from the subject and content.</param>
    /// <returns>An enumerable collection of extracted data.</returns>
    private static IEnumerable<TData> ExtractData<TData>(
        string? subject,
        string content,
        Func<string, IEnumerable<TData>> extractionMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        IEnumerable<TData> subjectUrls = [];
        if (subject is not null)
        {
            subjectUrls = extractionMethod(subject);
        }

        IEnumerable<TData> contentUrls = extractionMethod(content);

        return subjectUrls.Union(contentUrls);
    }

    /// <summary>
    /// Attempts to create a <see cref="MailAddress"/> object from the sender information in the provided message.
    /// </summary>
    /// <param name="message">The message containing the sender information.</param>
    /// <param name="senderEmail">
    /// When this method returns, contains the <see cref="MailAddress"/> object created from the sender information,
    /// if the sender is a valid email address; otherwise, null.
    /// </param>
    /// <returns>
    /// <c>true</c> if the sender information is a valid email address and a <see cref="MailAddress"/> object was created;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool TryCreateSenderEmail(
        Message message,
        [NotNullWhen(true)] out MailAddress? senderEmail)
    {
        senderEmail = null;
        return message.Type is MessageType.Email
            && MailAddress.TryCreate(message.Sender, out senderEmail);
    }

    /// <summary>
    /// Converts an array of `FileData` objects into an array of their associated `Stream` objects.
    /// </summary>
    /// <param name="files">An optional array of `FileData` objects containing file streams.</param>
    /// <returns>
    /// An array of `Stream` objects extracted from the provided `FileData` objects.
    /// Returns an empty array if `files` is null.
    /// </returns>
    private static Stream[] GetFileDataStreams(FileData[]? files)
    {
        return files is null
            ? []
            : files.Select(f => f.Stream).ToArray();
    }

    /// <summary>
    /// Fetches the most recent analysis for the given message, if it exists.
    /// </summary>
    /// <param name="message">The message for which to fetch the last analysis.</param>
    /// <param name="filesData">An array of streams representing the file data associated with the message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the most recent
    /// <see cref="MessageAnalysis"/> if found; otherwise, <c>null</c>.
    /// </returns>
    private async Task<MessageAnalysis?> FetchLastAnalysisAsync(
        Message message,
        Stream[] filesData,
        CancellationToken cancellationToken)
    {
        ContentHashSet messageHashSet = await _messageAnalysisBuilder
            .GenerateHashAsync(
                message,
                filesData,
                cancellationToken);

        IReadOnlyList<MessageAnalysis> existingAnalyses = await _repository.GetManyAsync(
            amount: 1,
            filter: m => m.Message.MessageHashSet == messageHashSet,
            orderBy: q => q.OrderByDescending(m => m.StartedDate),
            cancellationToken);

        return existingAnalyses.FirstOrDefault();
    }
}