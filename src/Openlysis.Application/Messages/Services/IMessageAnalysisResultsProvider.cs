using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

namespace Openlysis.Application.Messages.Services;

/// <summary>
/// Defines a provider of analysis and reputation results for data of a message.
/// </summary>
public interface IMessageAnalysisResultsProvider
{
    /// <summary>
    /// Retrieves a list of file multi-analyses for the given message analysis.
    /// </summary>
    /// <param name="messageAnalysis">The message analysis to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only list of file multi-analyses.
    /// </returns>
    public Task<IReadOnlyList<FileMultiAnalysis>> GetFileMultiAnalysesAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of URL multi-analyses for the given message analysis.
    /// </summary>
    /// <param name="messageAnalysis">The message analysis to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only list of URL multi-analyses.
    /// </returns>
    public Task<IReadOnlyList<UrlMultiAnalysis>> GetUrlMultiAnalysesAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of email address reputations for the given message analysis.
    /// </summary>
    /// <param name="messageAnalysis">The message analysis to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only list of email address reputations.
    /// </returns>
    public Task<IReadOnlyList<EmailAddressMultiReputation>> GetEmailAddressesReputationsAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of phone number reputations for the given message analysis.
    /// </summary>
    /// <param name="messageAnalysis">The message analysis to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a read-only list of phone number reputations.
    /// </returns>
    public Task<IReadOnlyList<PhoneMultiReputation>> GetPhoneNumbersReputationsAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default);
}