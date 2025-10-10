using System.Net.Mail;

using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Common.Models;
using Openlysis.Application.EmailAddresses.Services;
using Openlysis.Application.Files.Services;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Application.Phones.Services;
using Openlysis.Application.URLs.Services;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// Provides methods for analyzing data related to messages.
/// Implements the <see cref="IMessageAnalyzer"/> interface.
/// </summary>
internal sealed class MessageAnalyzer : IMessageAnalyzer
{
    // TODO: Add IsAvailable to file and url analysis services.

    /// <inheritdoc/>
    public bool IsAvailable => _phoneReputationService.IsAvailable
        && _emailAddressReputationService.IsAvailable;

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
    public async ValueTask<IEnumerable<FileMultiAnalysis>> AnalyzeFilesAsync(
        GlobalId userId,
        bool isPrivate,
        bool reanalyze,
        GlobalId correlationId,
        ProcessedFile[] files,
        Dictionary<ProcessedFile, string> filePasswords,
        CancellationToken cancellationToken = default)
    {
        if (files.Length is 0)
        {
            return [];
        }

        List<FileMultiAnalysis> fileMultiAnalyses = [];
        foreach (var file in files)
        {
            filePasswords.TryGetValue(file, out string? password);
            var requestResult = await _fileAnalysisService.AnalyzeAsync(
                userId,
                file,
                password ?? string.Empty,
                isPrivate,
                reanalyze,
                cancellationToken,
                correlationId);

            if (requestResult.IsError)
            {
                _logger.LogError(
                    "File could not be analyzed due to one or more errors."
                    + "\n\tFile Name: {FileName}"
                    + "\n\tFile Content Type: {FileContentType}"
                    + "\n\tFile Size: {FileSize}"
                    + "\n\tErrors: {Errors}",
                    file.Metadata.Name,
                    file.Metadata.ContentType,
                    file.Metadata.Size,
                    requestResult.Errors);
                continue;
            }

            fileMultiAnalyses.Add(requestResult.Value.Analysis);
        }

        return fileMultiAnalyses;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<UrlMultiAnalysis>> AnalyzeUrlsAsync(
        GlobalId userId,
        bool isPrivate,
        bool reanalyze,
        GlobalId correlationId,
        Uri[] urls,
        CancellationToken cancellationToken = default)
    {
        if (urls.Length is 0)
        {
            return [];
        }

        List<UrlMultiAnalysis> urlMultiAnalyses = [];
        foreach (var url in urls)
        {
            var requestResult = await _urlAnalysisService.AnalyzeAsync(
                userId,
                isPrivate,
                url,
                reanalyze,
                cancellationToken,
                correlationId);

            if (requestResult.IsError)
            {
                _logger.LogError(
                    "Url could not be analyzed due to one or more errors."
                    + "\n\tUrl: {Url}."
                    + "\n\tErrors: {Errors}.",
                    url,
                    requestResult.Errors);
                continue;
            }

            urlMultiAnalyses.Add(requestResult.Value.Analysis);
        }

        return urlMultiAnalyses;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<EmailAddressMultiReputation>> GetEmailAddressesReputationsAsync(
        MailAddress[] emailAddresses,
        CancellationToken cancellationToken = default)
    {
        if (emailAddresses.Length is 0)
        {
            return [];
        }

        List<EmailAddressMultiReputation> multiReputations = [];
        foreach (var email in emailAddresses)
        {
            ErrorOr<EmailAddressMultiReputation> result = await _emailAddressReputationService.GetAsync(
                email,
                storeInDatabase: true,
                cancellationToken);

            if (result.IsError)
            {
                _logger.LogError(
                    "Email address could not be analyzed due to one or more errors."
                    + "\n\tEmail Address: {EmailAddress}."
                    + "\n\tErrors: {Errors}.",
                    email,
                    result.Errors);
                continue;
            }

            multiReputations.Add(result.Value);
        }

        return multiReputations;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<PhoneMultiReputation>> GetPhoneNumbersReputationsAsync(
        string[] phoneNumbers,
        CancellationToken cancellationToken = default)
    {
        if (phoneNumbers.Length is 0)
        {
            return [];
        }

        List<PhoneMultiReputation> multiReputations = [];
        foreach (var phone in phoneNumbers)
        {
            ErrorOr<PhoneMultiReputation> result = await _phoneReputationService.GetAsync(
                phone,
                storeInDatabase: true,
                cancellationToken);

            if (result.IsError)
            {
                _logger.LogError(
                    "Phone number could not be analyzed due to one or more errors."
                    + "\n\tPhone Number: {PhoneNumber}."
                    + "\n\tErrors: {Errors}.",
                    phone,
                    result.Errors);
                continue;
            }

            multiReputations.Add(result.Value);
        }

        return multiReputations;
    }
}