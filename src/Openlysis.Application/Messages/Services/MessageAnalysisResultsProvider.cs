using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

namespace Openlysis.Application.Messages.Services;

/// <summary>
/// Provides methods to retrieve analysis results for files, URLs, email addresses, and phone numbers
/// associated with a <see cref="MessageAnalysis"/>.
/// </summary>
internal sealed class MessageAnalysisResultsProvider : IMessageAnalysisResultsProvider
{
    private readonly IRepository<FileMultiAnalysis, GlobalId> _fileMultiAnalysisRepository;
    private readonly IRepository<UrlMultiAnalysis, GlobalId> _urlMultiAnalysisRepository;
    private readonly IRepository<EmailAddressMultiReputation, GlobalId> _emailReputationsRepository;
    private readonly IRepository<PhoneMultiReputation, GlobalId> _phoneReputationsRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisResultsProvider"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">
    /// The repository for retrieving file multi-analysis results.
    /// </param>
    /// <param name="urlMultiAnalysisRepository">
    /// The repository for retrieving URL multi-analysis results.
    /// </param>
    /// <param name="emailReputationsRepository">
    /// The repository for retrieving email address reputation results.
    /// </param>
    /// <param name="phoneReputationsRepository">
    /// The repository for retrieving phone number reputation results.
    /// </param>
    public MessageAnalysisResultsProvider(
        IRepository<FileMultiAnalysis, GlobalId> fileMultiAnalysisRepository,
        IRepository<UrlMultiAnalysis, GlobalId> urlMultiAnalysisRepository,
        IRepository<EmailAddressMultiReputation, GlobalId> emailReputationsRepository,
        IRepository<PhoneMultiReputation, GlobalId> phoneReputationsRepository)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
        _urlMultiAnalysisRepository = urlMultiAnalysisRepository;
        _emailReputationsRepository = emailReputationsRepository;
        _phoneReputationsRepository = phoneReputationsRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileMultiAnalysis>> GetFileMultiAnalysesAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        GlobalId[] ids = messageAnalysis.AttachedFilesResults
            .Select(a => a.ResultId)
            .ToArray();

        return await _fileMultiAnalysisRepository.GetManyByIdsAsync(ids, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetUrlMultiAnalysesAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        GlobalId[] ids = messageAnalysis.DetectedUrlsResults
            .Select(a => a.ResultId)
            .ToArray();

        return await _urlMultiAnalysisRepository.GetManyByIdsAsync(ids, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmailAddressMultiReputation>> GetEmailAddressesReputationsAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        GlobalId[] ids = messageAnalysis.DetectedEmailAddressesResults
            .Select(a => a.ResultId)
            .ToArray();

        return await _emailReputationsRepository.GetManyByIdsAsync(ids, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PhoneMultiReputation>> GetPhoneNumbersReputationsAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        GlobalId[] ids = messageAnalysis.DetectedPhoneNumbersResults
            .Select(a => a.ResultId)
            .ToArray();

        return await _phoneReputationsRepository.GetManyByIdsAsync(ids, cancellationToken);
    }
}