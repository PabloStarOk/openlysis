using System.Text;

using ErrorOr;

using Microsoft.IO;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Enums;
using Openlysis.Application.URLs.Contracts.Abstractions;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.URLs.Services;

/// <summary>
/// Service for analyzing a URL with multiple analyzers.
/// Implements the <see cref="IUrlMultiAnalysisService"/> interface.
/// </summary>
internal class UrlMultiAnalysisService : IUrlMultiAnalysisService
{
    private readonly IRepository<UrlMultiAnalysis, GlobalId> _repository;
    private readonly TimeProvider _timeProvider;
    private readonly IHashService _hashService;
    private readonly IUrlMultiAnalyzer _urlMultiAnalyzer;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="repository">The repository for managing <see cref="UrlMultiAnalysis"/> entities.</param>
    /// <param name="timeProvider">The service for providing the current time.</param>
    /// <param name="hashService">The service for generating and managing hashes.</param>
    /// <param name="urlMultiAnalyzer">The service for performing multi-analysis on URLs.</param>
    /// <param name="memoryStreamManager">The manager for recyclable <see cref="MemoryStream"/> used for storing URL string bytes.</param>
    public UrlMultiAnalysisService(
        IRepository<UrlMultiAnalysis, GlobalId> repository,
        TimeProvider timeProvider,
        IHashService hashService,
        IUrlMultiAnalyzer urlMultiAnalyzer,
        RecyclableMemoryStreamManager memoryStreamManager)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _hashService = hashService;
        _urlMultiAnalyzer = urlMultiAnalyzer;
        _memoryStreamManager = memoryStreamManager;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<UrlMultiAnalysis>> AnalyzeAsync(
        UserId userId,
        bool isPrivate,
        Uri url,
        bool reanalyze,
        CancellationToken cancellationToken)
    {
        HashValues urlHashValues;
        byte[] urlBytes = Encoding.UTF8.GetBytes(url.AbsoluteUri);
        await using (var urlMemoryStream = _memoryStreamManager.GetStream(urlBytes))
        {
            urlHashValues = await _hashService.HashDataAsync(urlMemoryStream, cancellationToken);
        }

        IReadOnlyList<UrlMultiAnalysis> lastExistingAnalyses = await GetAnalysesByHashAsync(
            userId,
            hash: urlHashValues.Sha256,
            pageSize: 1,
            size: 1,
            order: OrderType.Dsc,
            cancellationToken);
        if (lastExistingAnalyses.Count > 0
            && !reanalyze)
        {
            return lastExistingAnalyses[0];
        }

        var multiAnalysis = UrlMultiAnalysis.Create(
            userId,
            isPrivate,
            _timeProvider.GetUtcNow().UtcDateTime,
            url,
            urlHashValues);

        await _urlMultiAnalyzer.StartAnalysisAsync(
            multiAnalysis.Id,
            url,
            cancellationToken);

        await _repository.AddAsync(multiAnalysis, cancellationToken);
        return multiAnalysis;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<UrlMultiAnalysis>> GetAnalysisByIdAsync(
        UserId userId,
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        UrlMultiAnalysis? multiAnalysis = await _repository.GetAsync(id, cancellationToken);
        if (multiAnalysis is null)
        {
            return Error.NotFound();
        }

        if (multiAnalysis.IsPrivate &&
            multiAnalysis.UserId != userId)
        {
            return Error.NotFound();
        }

        return multiAnalysis;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetAnalysesByHashAsync(
        UserId userId,
        string hash,
        int pageSize,
        int size,
        OrderType order,
        CancellationToken cancellationToken = default)
    {
        var analyses = await _repository.GetManyAsync(
            pageSize,
            size,
            u => (u.DataHashValues.Sha256 == hash
                    || u.DataHashValues.Md5 == hash
                    || u.DataHashValues.Sha1 == hash
                    || u.DataHashValues.Sha512 == hash)
                && (!u.IsPrivate || (u.IsPrivate && u.UserId == userId)),
            OrderBy,
            cancellationToken);

        return analyses.ToList().AsReadOnly();

        // Order
        IOrderedQueryable<UrlMultiAnalysis> OrderBy(IQueryable<UrlMultiAnalysis> q)
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
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetAnalysesByUserAsync(
        UserId userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // TODO: Duplicated logic with file and message services.
        var analyses = await _repository.GetManyAsync(
            page,
            pageSize,
            filter: a => a.UserId == userId,
            cancellationToken: cancellationToken);

        return analyses.ToList().AsReadOnly();
    }
}
