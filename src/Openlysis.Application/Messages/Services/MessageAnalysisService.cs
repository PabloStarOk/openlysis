using System.Net.Mail;

using ErrorOr;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Enums;
using Openlysis.Application.Common.Models;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.Messages.Contracts.Requests;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Enums;
using Openlysis.Domain.Messages.ValueObjects;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

namespace Openlysis.Application.Messages.Services;

/// <summary>
/// Service for analyzing a messages and retrieving the analyses.
/// </summary>
internal class MessageAnalysisService : IMessageAnalysisService
{
    /// <inheritdoc/>
    public bool AnalyzeIsAvailable => _messageAnalyzer.IsAvailable;

    private readonly IRepository<MessageAnalysis> _repository;
    private readonly TimeProvider _timeProvider;
    private readonly IMessageHashService _messageHashService;
    private readonly IMessageDataExtractor _dataExtractor;
    private readonly IMessageAnalyzer _messageAnalyzer;
    private readonly IMessageAnalysisQueue _messageAnalysisQueue;
    private readonly IRecentAnalysisFinder<MessageAnalysis> _recentAnalysisFinder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisService"/> class.
    /// </summary>
    /// <param name="repository">The repository for managing <see cref="MessageAnalysis"/> entities.</param>
    /// <param name="timeProvider">Provides the current time for timestamping analyses.</param>
    /// <param name="messageHashService">Service for generating message hash values.</param>
    /// <param name="dataExtractor">The service responsible for extracting data (e.g., URLs, email addresses, phone numbers) from messages.</param>
    /// <param name="messageAnalyzer">The service responsible for analyzing messages and their associated data.</param>
    /// <param name="messageAnalysisQueue">Queue for managing asynchronous message analysis operations.</param>
    /// <param name="recentAnalysisFinder">Finder for the most recent analysis of a message.</param>
    public MessageAnalysisService(
        IRepository<MessageAnalysis> repository,
        TimeProvider timeProvider,
        IMessageHashService messageHashService,
        IMessageDataExtractor dataExtractor,
        IMessageAnalyzer messageAnalyzer,
        IMessageAnalysisQueue messageAnalysisQueue,
        IRecentAnalysisFinder<MessageAnalysis> recentAnalysisFinder)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _messageHashService = messageHashService;
        _dataExtractor = dataExtractor;
        _messageAnalyzer = messageAnalyzer;
        _messageAnalysisQueue = messageAnalysisQueue;
        _recentAnalysisFinder = recentAnalysisFinder;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<AnalysisRequestResult<MessageAnalysis>>> AnalyzeAsync(
        GlobalId userId,
        bool isPrivate,
        Message message,
        ProcessedFile[] files,
        Dictionary<ProcessedFile, string> filePasswords,
        bool reanalyze,
        string? requestCountryCode,
        CancellationToken cancellationToken = default)
    {
        if (!AnalyzeIsAvailable)
        {
            return Error.Failure("Service is not available");
        }

        HashValues messageHashValues = await _messageHashService.HashAsync(message, files, cancellationToken);
        ReusableAnalysis<MessageAnalysis> reusableAnalysis = await _recentAnalysisFinder
            .FindMostRecentAsync(userId, messageHashValues, cancellationToken);

        if (reusableAnalysis.IsReusable && !reanalyze)
        {
            return new AnalysisRequestResult<MessageAnalysis>(
                AnalysisRequestStatus.Retrieved,
                reusableAnalysis.Analysis);
        }

        string sender = message.Sender;
        string? subject = message.Subject;
        string content = message.Content;

        HashValues reusedHashValues = reusableAnalysis.Analysis?.Message.HashValues ?? messageHashValues;
        var messageInformation = new MessageInformation(
            message.Type,
            message.Sender,
            message.Subject,
            message.Content,
            reusedHashValues);

        var correlationId = GlobalId.CreateUnique();
        var messageAnalysis = MessageAnalysis.Create(
            _timeProvider.GetUtcNow(),
            userId,
            isPrivate,
            messageInformation);

        if (requestCountryCode is not null)
        {
            _dataExtractor.SetRequestCountryCode(requestCountryCode);
        }

        Uri[] urls = ExtractDataFromValidInputs(
            _dataExtractor.ExtractUrls, subject, content);
        MailAddress[] emailAddresses = ExtractDataFromValidInputs(
            _dataExtractor.ExtractEmailAddresses, sender, subject, content);
        string[] phoneNumbers = ExtractDataFromValidInputs(
            _dataExtractor.ExtractPhoneNumbers, subject, content);

        IEnumerable<EmailAddressMultiReputation> emailAddressesReputations =
            await _messageAnalyzer.GetEmailAddressesReputationsAsync(emailAddresses, cancellationToken);
        IEnumerable<PhoneMultiReputation> phoneNumbersReputations =
            await _messageAnalyzer.GetPhoneNumbersReputationsAsync(phoneNumbers, cancellationToken);

        messageAnalysis.AddEmailAddressResults(emailAddressesReputations.ToArray());
        messageAnalysis.AddPhoneNumberResults(phoneNumbersReputations.ToArray());

        if (files.Length is 0 && urls.Length is 0)
        {
            messageAnalysis.CompleteInitialization();
            await _repository.AddAsync(messageAnalysis, cancellationToken);
            await _repository.SaveChangeAsync(cancellationToken);
            return new AnalysisRequestResult<MessageAnalysis>(
                AnalysisRequestStatus.Retrieved,
                messageAnalysis);
        }

        await _messageAnalysisQueue.EnqueueAsync(correlationId, messageAnalysis, cancellationToken);

        IEnumerable<FileMultiAnalysis> fileMultiAnalyses =
            await _messageAnalyzer.AnalyzeFilesAsync(userId, isPrivate, reanalyze, correlationId, files, filePasswords, cancellationToken);
        IEnumerable<UrlMultiAnalysis> urlMultiAnalyses =
            await _messageAnalyzer.AnalyzeUrlsAsync(userId, isPrivate, reanalyze, correlationId, urls, cancellationToken);

        messageAnalysis.AddFileResults(fileMultiAnalyses.ToArray());
        messageAnalysis.AddUrlResults(urlMultiAnalyses.ToArray());
        messageAnalysis.CompleteInitialization();

        await _repository.AddAsync(messageAnalysis, cancellationToken);
        await _repository.SaveChangeAsync(cancellationToken);
        await _messageAnalysisQueue.SetAsInitializedAsync(correlationId, cancellationToken);

        return new AnalysisRequestResult<MessageAnalysis>(
            AnalysisRequestStatus.Queued,
            messageAnalysis);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<MessageAnalysis>> GetAnalysisByIdAsync(
        GlobalId userId,
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        MessageAnalysis? messageAnalysis = await _repository.GetAsync(id, cancellationToken);
        if (messageAnalysis is null)
        {
            return Error.NotFound();
        }

        if (messageAnalysis.IsPrivate && messageAnalysis.UserId != userId)
        {
            return Error.NotFound();
        }

        return messageAnalysis;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MessageAnalysis>> GetAnalysesByHashAsync(
        GlobalId userId,
        string hash,
        int page,
        int pageSize,
        OrderType order,
        CancellationToken cancellationToken = default)
    {
        var analyses = await _repository.GetManyAsync(
            page,
            pageSize,
            u => (u.Message.HashValues.Sha256 == hash
                    || u.Message.HashValues.Md5 == hash
                    || u.Message.HashValues.Sha1 == hash
                    || u.Message.HashValues.Sha512 == hash)
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

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MessageAnalysis>> GetAnalysesByUserAsync(
        GlobalId userId,
        MessageType messageType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // TODO: Duplicated logic with url and file services.
        var analyses = await _repository.GetManyAsync(
            page,
            pageSize,
            filter: a => a.UserId == userId && a.Message.Type == messageType,
            cancellationToken: cancellationToken);

        return analyses.ToList().AsReadOnly();
    }

    /// <summary>
    /// Extracts data from the provided input strings using the specified extraction method.
    /// </summary>
    /// <typeparam name="TData">The type of data to extract.</typeparam>
    /// <param name="extractionMethod">
    /// The function used to extract data from each input string.
    /// </param>
    /// <param name="inputs">
    /// The input strings to extract data from.
    /// </param>
    /// <returns>
    /// An enumerable collection containing all extracted data from the provided inputs.
    /// </returns>
    private static TData[] ExtractDataFromValidInputs<TData>(
        Func<string, IEnumerable<TData>> extractionMethod,
        params string?[] inputs)
    {
        return inputs
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .SelectMany(extractionMethod!)
            .ToHashSet()
            .ToArray();
    }
}