using System.Net.Mail;

using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Application.EmailAddresses.Contracts.Requests;
using Openlysis.Application.EmailAddresses.Services;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Files.Services;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.Phones.Contracts.Requests;
using Openlysis.Application.Phones.Services;
using Openlysis.Application.URLs.Services;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// Provides methods for analyzing data related to messages.
/// Implements the <see cref="IMessageAnalyzer"/> interface.
/// </summary>
internal sealed class MessageAnalyzer : IMessageAnalyzer
{
    private readonly ILogger<MessageAnalyzer> _logger;
    private readonly IFileMultiAnalysisService _fileAnalysisService;
    private readonly IUrlMultiAnalysisService _urlAnalysisService;
    private readonly IPhoneReputationService _phoneReputationService;
    private readonly IEmailAddressReputationService _emailAddressReputationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging messages.</param>
    /// <param name="fileAnalysisService">Service for analyzing files.</param>
    /// <param name="urlAnalysisService">Service for analyzing URLs.</param>
    /// <param name="phoneReputationService">Service for assessing phone reputations.</param>
    /// <param name="emailAddressReputationService">Service for assessing email address reputations.</param>
    public MessageAnalyzer(
        ILogger<MessageAnalyzer> logger,
        IFileMultiAnalysisService fileAnalysisService,
        IUrlMultiAnalysisService urlAnalysisService,
        IPhoneReputationService phoneReputationService,
        IEmailAddressReputationService emailAddressReputationService)
    {
        _logger = logger;
        _fileAnalysisService = fileAnalysisService;
        _urlAnalysisService = urlAnalysisService;
        _phoneReputationService = phoneReputationService;
        _emailAddressReputationService = emailAddressReputationService;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FileMultiAnalysis>> AnalyzeFilesAsync(
        UserId userId,
        bool isPrivate,
        bool reanalyzeData,
        FileData[] files,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);

        List<FileMultiAnalysis> fileMultiAnalyses = [];
        await Parallel.ForEachAsync(files, cancellationToken, async (file, ct) =>
        {
            ErrorOr<FileMultiAnalysis> result = await _fileAnalysisService.AnalyzeAsync(
                userId,
                isPrivate,
                reanalyzeData,
                file,
                ct);

            if (result.IsError)
            {
                _logger.LogError(
                    "File could not be analyzed due to one or more errors."
                    + "\n\tFile Name: {FileName}"
                    + "\n\tFile Content Type: {FileContentType}"
                    + "\n\tFile Size: {FileSize}"
                    + "\n\tErrors: {Errors}",
                    file.Name,
                    file.ContentType,
                    file.Stream.Length,
                    result.Errors);
                return;
            }

            fileMultiAnalyses.Add(result.Value);
        });

        return fileMultiAnalyses;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UrlMultiAnalysis>> AnalyzeUrlsAsync(
        UserId userId,
        bool isPrivate,
        IEnumerable<Uri> urls,
        CancellationToken cancellationToken = default)
    {
        List<UrlMultiAnalysis> urlMultiAnalyses = [];
        await Parallel.ForEachAsync(urls, cancellationToken, async (url, ct) =>
        {
            ErrorOr<UrlMultiAnalysis> result = await _urlAnalysisService.AnalyzeAsync(
                userId,
                isPrivate,
                url,
                ct);

            if (result.IsError)
            {
                _logger.LogError(
                    "Url could not be analyzed due to one or more errors."
                    + "\n\tUrl: {Url}."
                    + "\n\tErrors: {Errors}.",
                    url,
                    result.Errors);
                return;
            }

            urlMultiAnalyses.Add(result.Value);
        });

        return urlMultiAnalyses;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EmailAddressMultiReputation>> GetEmailAddressesReputationsAsync(
        IEnumerable<MailAddress> emailAddresses,
        CancellationToken cancellationToken = default)
    {
        List<EmailAddressMultiReputation> multiReputations = [];
        await Parallel.ForEachAsync(
            emailAddresses,
            cancellationToken,
            async (email, ct) =>
        {
            var request = new EvaluateEmailAddressReputation(email.Address);
            ErrorOr<EmailAddressMultiReputation> result = await _emailAddressReputationService.GetAsync(
                request,
                storeInDatabase: true,
                ct);

            if (result.IsError)
            {
                _logger.LogError(
                    "Email address could not be analyzed due to one or more errors."
                    + "\n\tEmail Address: {EmailAddress}."
                    + "\n\tErrors: {Errors}.",
                    email,
                    result.Errors);
                return;
            }

            multiReputations.Add(result.Value);
        });

        return multiReputations;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PhoneMultiReputation>> GetPhoneNumbersReputationsAsync(
        IEnumerable<string> phoneNumbers,
        CancellationToken cancellationToken = default)
    {
        List<PhoneMultiReputation> multiReputations = [];
        await Parallel.ForEachAsync(
            phoneNumbers,
            cancellationToken,
            async (phone, ct) =>
            {
                var request = new EvaluatePhoneReputation(phone);
                ErrorOr<PhoneMultiReputation> result = await _phoneReputationService.AssessAsync(
                    request,
                    storeInDatabase: true,
                    ct);

                if (result.IsError)
                {
                    _logger.LogError(
                    "Phone number could not be analyzed due to one or more errors."
                    + "\n\tPhone Number: {PhoneNumber}."
                    + "\n\tErrors: {Errors}.",
                    phone,
                    result.Errors);
                    return;
                }

                multiReputations.Add(result.Value);
            });

        return multiReputations;
    }
}