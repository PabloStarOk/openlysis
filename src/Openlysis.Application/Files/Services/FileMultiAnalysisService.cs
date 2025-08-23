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
    private readonly IRepository<FileMultiAnalysis, GlobalId> _repository;
    private readonly TimeProvider _timeProvider;
    private readonly IFileMultiAnalyzer _multiAnalyzer;
    private readonly IFileStorageContext _fileStorageContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="repository">Repository for <see cref="FileMultiAnalysis"/> entities.</param>
    /// <param name="multiAnalyzer">Service for performing multiple file analyses.</param>
    /// <param name="timeProvider">Provides the current time.</param>
    /// <param name="fileStorageContext">Context for file storage operations.</param>
    public FileMultiAnalysisService(
        IRepository<FileMultiAnalysis, GlobalId> repository,
        IFileMultiAnalyzer multiAnalyzer,
        TimeProvider timeProvider,
        IFileStorageContext fileStorageContext)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _multiAnalyzer = multiAnalyzer;
        _fileStorageContext = fileStorageContext;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<FileMultiAnalysis>> AnalyzeAsync(
        GlobalId userId,
        ProcessedFile processedFile,
        string filePassword,
        bool isPrivate,
        bool reanalyze,
        CancellationToken cancellationToken = default)
    {
        // Check if the file has already been analyzed.
        var existingAnalyses = await _repository.GetManyAsync(
            page: 1,
            pageSize: 1,
            f => f.DataHashValues == processedFile.HashValues,
            q => q.OrderByDescending(f => f.StartedDate),
            cancellationToken);

        if (existingAnalyses.Count > 0 && !reanalyze)
        {
            await _fileStorageContext.RemoveAsync(processedFile);
            return existingAnalyses[0];
        }

        var multiAnalysis = FileMultiAnalysis.Create(
            userId,
            isPrivate,
            _timeProvider.GetUtcNow().UtcDateTime,
            processedFile.HashValues,
            processedFile.Metadata);

        await _repository.AddAsync(multiAnalysis, cancellationToken);
        await _multiAnalyzer.StartAnalysisAsync(
            multiAnalysis.Id,
            processedFile,
            filePassword,
            isPrivate,
            cancellationToken);

        return multiAnalysis;
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

        if (multiAnalysis.IsPrivate
            && multiAnalysis.UserId != userId)
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