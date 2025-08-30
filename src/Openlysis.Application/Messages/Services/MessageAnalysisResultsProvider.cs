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
    private readonly IRepository<FileMultiAnalysis> _fileMultiAnalysisRepository;
    private readonly IRepository<UrlMultiAnalysis> _urlMultiAnalysisRepository;
    private readonly IRepository<EmailAddressMultiReputation> _emailReputationsRepository;
    private readonly IRepository<PhoneMultiReputation> _phoneReputationsRepository;

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
        IRepository<FileMultiAnalysis> fileMultiAnalysisRepository,
        IRepository<UrlMultiAnalysis> urlMultiAnalysisRepository,
        IRepository<EmailAddressMultiReputation> emailReputationsRepository,
        IRepository<PhoneMultiReputation> phoneReputationsRepository)
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
        GlobalId[] ids = messageAnalysis.AttachedFilesIndicators
            .Select(a => a.ResultId)
            .ToArray();

        if (ids.Length is 0)
        {
            return [];
        }

        return await _fileMultiAnalysisRepository.GetManyByIdsAsync(ids, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetUrlMultiAnalysesAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        GlobalId[] ids = messageAnalysis.DetectedUrlsIndicators
            .Select(a => a.ResultId)
            .ToArray();

        if (ids.Length is 0)
        {
            return [];
        }

        return await _urlMultiAnalysisRepository.GetManyByIdsAsync(ids, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmailAddressMultiReputation>> GetEmailAddressesReputationsAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        GlobalId[] ids = messageAnalysis.DetectedEmailAddressesIndicators
            .Select(a => a.ResultId)
            .ToArray();

        if (ids.Length is 0)
        {
            return [];
        }

        return await _emailReputationsRepository.GetManyByIdsAsync(ids, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PhoneMultiReputation>> GetPhoneNumbersReputationsAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        GlobalId[] ids = messageAnalysis.DetectedPhoneNumbersIndicators
            .Select(a => a.ResultId)
            .ToArray();

        if (ids.Length is 0)
        {
            return [];
        }

        return await _phoneReputationsRepository.GetManyByIdsAsync(ids, cancellationToken);
    }
}