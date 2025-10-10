using ErrorOr;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Enums;
using Openlysis.Application.Common.Models;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.Application.Files.Services;

/// <summary>
/// Service for analyzing a file with multiple analyzers
/// Implements the <see cref="IFileMultiAnalysisService"/> interface.
/// </summary>
internal class FileMultiAnalysisService : IFileMultiAnalysisService
{
    private readonly IRepository<FileMultiAnalysis> _repository;
    private readonly TimeProvider _timeProvider;
    private readonly IFileMultiAnalysisQueue _multiAnalysisQueue;
    private readonly IFileStorageContext _fileStorageContext;
    private readonly IRecentAnalysisFinder<FileMultiAnalysis> _recentAnalysisFinder;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="repository">Repository for <see cref="FileMultiAnalysis"/> entities.</param>
    /// <param name="multiAnalysisQueue">Queue for multi-analysis operations.</param>
    /// <param name="timeProvider">Provides the current time.</param>
    /// <param name="fileStorageContext">Context for file storage operations.</param>
    /// <param name="recentAnalysisFinder">Finder for the most recent analysis of a file.</param>
    public FileMultiAnalysisService(
        IRepository<FileMultiAnalysis> repository,
        IFileMultiAnalysisQueue multiAnalysisQueue,
        TimeProvider timeProvider,
        IFileStorageContext fileStorageContext,
        IRecentAnalysisFinder<FileMultiAnalysis> recentAnalysisFinder)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _multiAnalysisQueue = multiAnalysisQueue;
        _fileStorageContext = fileStorageContext;
        _recentAnalysisFinder = recentAnalysisFinder;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<AnalysisRequestResult<FileMultiAnalysis>>> AnalyzeAsync(
        GlobalId userId,
        ProcessedFile processedFile,
        string filePassword,
        bool isPrivate,
        bool reanalyze,
        CancellationToken cancellationToken = default,
        GlobalId? correlationId = null)
    {
        ReusableAnalysis<FileMultiAnalysis> reusableAnalysis = await _recentAnalysisFinder
            .FindMostRecentAsync(userId, processedFile.HashValues, cancellationToken);

        if (reusableAnalysis.IsReusable && !reanalyze)
        {
            await _fileStorageContext.RemoveAsync(processedFile);
            return new AnalysisRequestResult<FileMultiAnalysis>(
                AnalysisRequestStatus.Retrieved,
                reusableAnalysis.Analysis);
        }

        var reusedHashValues = reusableAnalysis.Analysis?.DataHashValues ?? processedFile.HashValues;
        var multiAnalysis = FileMultiAnalysis.Create(
            userId,
            isPrivate,
            _timeProvider.GetUtcNow(),
            reusedHashValues,
            processedFile.Metadata);

        await _repository.AddAsync(multiAnalysis, cancellationToken);
        await _repository.SaveChangeAsync(cancellationToken);
        await _multiAnalysisQueue.QueueAsync(
            multiAnalysis.Id,
            processedFile,
            filePassword,
            isPrivate,
            correlationId,
            cancellationToken);

        return new AnalysisRequestResult<FileMultiAnalysis>(
            AnalysisRequestStatus.Queued,
            multiAnalysis);
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<FileMultiAnalysis>> GetAnalysisByIdAsync(
        GlobalId userId,
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        var multiAnalysis = await _repository.GetAsync(id, cancellationToken);

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
    public async Task<IReadOnlyList<FileMultiAnalysis>> GetAnalysesByHashAsync(
        GlobalId userId,
        string hash,
        int page,
        int pageSize,
        OrderType order,
        CancellationToken cancellationToken = default)
    {
        var multiAnalyses = await _repository.GetManyAsync(
            page,
            pageSize,
            f => (f.DataHashValues.Sha256 == hash
                    || f.DataHashValues.Md5 == hash
                    || f.DataHashValues.Sha1 == hash
                    || f.DataHashValues.Sha512 == hash)
                && (!f.IsPrivate || (f.IsPrivate && f.UserId == userId)),
            OrderBy,
            cancellationToken);

        return multiAnalyses
            .ToList()
            .AsReadOnly();

        // Order
        IOrderedQueryable<FileMultiAnalysis> OrderBy(IQueryable<FileMultiAnalysis> q)
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
    public async Task<IReadOnlyList<FileMultiAnalysis>> GetAnalysesByUserAsync(
        GlobalId userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // TODO: Duplicated logic with url and message services.
        var analyses = await _repository.GetManyAsync(
            page,
            pageSize,
            filter: a => a.UserId == userId,
            cancellationToken: cancellationToken);

        return analyses.ToList().AsReadOnly();
    }
}