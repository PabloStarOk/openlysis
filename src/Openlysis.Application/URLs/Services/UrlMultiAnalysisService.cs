using System.Text;

using ErrorOr;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Enums;
using Openlysis.Application.Common.Models;
using Openlysis.Application.URLs.Contracts.Abstractions;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;

namespace Openlysis.Application.URLs.Services;

/// <summary>
/// Service for analyzing a URL with multiple analyzers.
/// Implements the <see cref="IUrlMultiAnalysisService"/> interface.
/// </summary>
internal class UrlMultiAnalysisService : IUrlMultiAnalysisService
{
    private readonly IRepository<UrlMultiAnalysis> _repository;
    private readonly TimeProvider _timeProvider;
    private readonly IHashService _hashService;
    private readonly IUrlMultiAnalysisQueue _multiAnalysisQueue;
    private readonly IRecentAnalysisFinder<UrlMultiAnalysis> _recentAnalysisFinder;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="repository">The repository for managing <see cref="UrlMultiAnalysis"/> entities.</param>
    /// <param name="timeProvider">The service for providing the current time.</param>
    /// <param name="hashService">The service for generating and managing hashes.</param>
    /// <param name="multiAnalysisQueue">Queue for multi-analysis operations.</param>
    /// <param name="recentAnalysisFinder">Finder for the most recent analysis of a URL.</param>
    public UrlMultiAnalysisService(
        IRepository<UrlMultiAnalysis> repository,
        TimeProvider timeProvider,
        IHashService hashService,
        IUrlMultiAnalysisQueue multiAnalysisQueue,
        IRecentAnalysisFinder<UrlMultiAnalysis> recentAnalysisFinder)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _hashService = hashService;
        _multiAnalysisQueue = multiAnalysisQueue;
        _recentAnalysisFinder = recentAnalysisFinder;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<AnalysisRequestResult<UrlMultiAnalysis>>> AnalyzeAsync(
        GlobalId userId,
        bool isPrivate,
        Uri url,
        bool reanalyze,
        CancellationToken cancellationToken = default,
        GlobalId? correlationId = null)
    {
        byte[] urlBytes = Encoding.UTF8.GetBytes(url.AbsoluteUri);
        HashValues urlHashValues = _hashService.HashData(urlBytes);

        ReusableAnalysis<UrlMultiAnalysis> reusableAnalysis = await _recentAnalysisFinder
            .FindMostRecentAsync(userId, urlHashValues, cancellationToken);

        if (reusableAnalysis.IsReusable && !reanalyze)
        {
            return new AnalysisRequestResult<UrlMultiAnalysis>(
                AnalysisRequestStatus.Retrieved,
                reusableAnalysis.Analysis);
        }

        HashValues reusedHashValues = reusableAnalysis.Analysis?.DataHashValues ?? urlHashValues;
        var multiAnalysis = UrlMultiAnalysis.Create(
            userId,
            isPrivate,
            _timeProvider.GetUtcNow().UtcDateTime,
            url,
            reusedHashValues);

        await _repository.AddAsync(multiAnalysis, cancellationToken);
        await _repository.SaveChangeAsync(cancellationToken);
        await _multiAnalysisQueue.QueueAsync(
            multiAnalysis.Id,
            url,
            correlationId,
            cancellationToken);

        return new AnalysisRequestResult<UrlMultiAnalysis>(
            AnalysisRequestStatus.Queued,
            multiAnalysis);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<UrlMultiAnalysis>> GetAnalysisByIdAsync(
        GlobalId userId,
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        UrlMultiAnalysis? multiAnalysis = await _repository.GetAsync(id, cancellationToken);
        if (multiAnalysis is null)
        {
            return Error.NotFound();
        }

        if (multiAnalysis.IsPrivate && multiAnalysis.UserId != userId)
        {
            return Error.NotFound();
        }

        return multiAnalysis;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetAnalysesByHashAsync(
        GlobalId userId,
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
        GlobalId userId,
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
