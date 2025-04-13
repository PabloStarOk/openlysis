using ErrorOr;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Enums;
using Openlysis.Application.Files.Contracts.Abstractions;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.ValueObjects;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.Files.Services;

/// <summary>
/// Service for analyzing a file with multiple analyzers
/// Implements the <see cref="IFileMultiAnalysisService"/> interface.
/// </summary>
internal class FileMultiAnalysisService : IFileMultiAnalysisService
{
    private readonly IRepository<FileMultiAnalysis, GlobalId> _repository;
    private readonly TimeProvider _timeProvider;
    private readonly IHashService _hashService;
    private readonly IFileMultiAnalyzer _multiAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisService"/> class.
    /// </summary>
    /// <param name="repository">The repository for managing <see cref="FileMultiAnalysis"/> entities.</param>
    /// <param name="multiAnalyzer">The service responsible for performing multi-file analysis.</param>
    /// <param name="timeProvider">The provider for retrieving the current time.</param>
    /// <param name="hashService">The service for generating and managing data hashes.</param>
    public FileMultiAnalysisService(
        IRepository<FileMultiAnalysis, GlobalId> repository,
        IFileMultiAnalyzer multiAnalyzer,
        TimeProvider timeProvider,
        IHashService hashService)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _hashService = hashService;
        _multiAnalyzer = multiAnalyzer;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<FileMultiAnalysis>> AnalyzeAsync(
        UserId userId,
        bool isPrivate,
        bool reanalyze,
        FileData fileData,
        CancellationToken cancellationToken = default)
    {
        var hashSet = await _hashService.HashDataAsync(fileData.Stream, cancellationToken);

        // Check if the file has already been analyzed.
        var existingAnalyses = await _repository.GetManyAsync(
            1,
            f => f.DataHashSet == hashSet,
            q => q.OrderByDescending(f => f.StartedDate),
            cancellationToken);

        if (existingAnalyses.Count > 0 && !reanalyze)
        {
            return existingAnalyses[0];
        }

        var fileMetadata = new FileMetadata(
            fileData.Name,
            fileData.ContentType,
            fileData.Stream.Length);

        var multiAnalysis = FileMultiAnalysis.Create(
            userId,
            isPrivate,
            _timeProvider.GetUtcNow().UtcDateTime,
            hashSet,
            fileMetadata);

        await _multiAnalyzer.StartAnalysisAsync(
            multiAnalysis,
            fileData.Stream,
            fileData.Description,
            fileData.Password,
            isPrivate,
            cancellationToken);

        return multiAnalysis;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<FileMultiAnalysis>> GetAnalysisByIdAsync(
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        var fileAnalysis = await _repository.GetAsync(id, cancellationToken);

        if (fileAnalysis is null)
        {
            return Error.NotFound();
        }

        return fileAnalysis;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileMultiAnalysis>> GetAnalysesByHashAsync(
        string hash,
        int amount,
        OrderType order,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FileMultiAnalysis> fileAnalyses = await _repository.GetManyAsync(
            amount,
            f => f.DataHashSet.Sha256 == hash
                || f.DataHashSet.Md5 == hash
                || f.DataHashSet.Sha1 == hash
                || f.DataHashSet.Sha512 == hash,
            OrderBy,
            cancellationToken);

        return fileAnalyses
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
}