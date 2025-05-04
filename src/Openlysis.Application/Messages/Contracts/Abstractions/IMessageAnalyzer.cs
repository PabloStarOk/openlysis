using System.Net.Mail;

using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Application.Messages.Services;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.Messages.Contracts.Abstractions;

/// <summary>
/// Defines an analyzer of messages.
/// </summary>
/// <remarks>
/// Helper for <see cref="IMessageAnalysisService"/>.
/// </remarks>
public interface IMessageAnalyzer
{
    /// <summary>
    /// Gets a value indicating whether the analyzer is available for use.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Analyzes the provided files.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="reanalyze">Specifies whether to reanalyze the message even if there is an existing analysis available to retrieve.</param>
    /// <param name="files">The array of files to be analyzed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of file analysis results.</returns>
    public Task<IEnumerable<FileMultiAnalysis>> AnalyzeFilesAsync(
        UserId userId,
        bool isPrivate,
        bool reanalyze,
        FileData[] files,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes the provided URLs.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="urls">The collection of URLs to be analyzed.</param>
    /// <param name="reanalyze">Specifies whether to reanalyze the URLs even if they were previously analyzed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of URL analysis results.</returns>
    public Task<IEnumerable<UrlMultiAnalysis>> AnalyzeUrlsAsync(
        UserId userId,
        bool isPrivate,
        IEnumerable<Uri> urls,
        bool reanalyze,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the reputations of the provided email addresses.
    /// </summary>
    /// <param name="emailAddresses">The collection of email addresses to retrieve reputations for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of email address reputations.</returns>
    public Task<IEnumerable<EmailAddressMultiReputation>> GetEmailAddressesReputationsAsync(
        IEnumerable<MailAddress> emailAddresses,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the reputations of the provided phone numbers.
    /// </summary>
    /// <param name="phoneNumbers">The collection of phone numbers to retrieve reputations for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of phone number reputations.</returns>
    public Task<IEnumerable<PhoneMultiReputation>> GetPhoneNumbersReputationsAsync(
        IEnumerable<string> phoneNumbers,
        CancellationToken cancellationToken = default);
}