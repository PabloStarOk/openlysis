using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository to access <see cref="FileMultiAnalysis"/>.
/// </summary>
public class FileMultiAnalysisRepository : IFileMultiAnalysisRepository
{
    private readonly AnalysesDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to be used by the repository.</param>
    public FileMultiAnalysisRepository(AnalysesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task AddAsync(FileMultiAnalysis fileMultiAnalysis, CancellationToken cancellationToken = default)
    {
        var hashSetExists = await _dbContext.ContentHashSets
            .AnyAsync(
                c => c.Sha256 == fileMultiAnalysis.ContentHashSet.Sha256,
                cancellationToken);

        if (hashSetExists)
        {
            _dbContext.Attach(fileMultiAnalysis.ContentHashSet).State = EntityState.Unchanged;
        }

        await _dbContext.AddAsync(fileMultiAnalysis, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetAsync(FileMultiAnalysisId fileMultiAnalysisId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileMultiAnalyses.FindAsync([fileMultiAnalysisId], cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetByHashAsync(ContentHashSet contentHashSet, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.FileMultiAnalyses.AnyAsync(cancellationToken))
        {
            return null;
        }

        var fileAnalysis = await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Sha256 == contentHashSet.Md5,
                cancellationToken)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Md5 == contentHashSet.Sha1,
                cancellationToken)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Sha1 == contentHashSet.Sha256,
                cancellationToken)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Sha512 == contentHashSet.Sha512,
                cancellationToken);

        return fileAnalysis;
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.FileMultiAnalyses.AnyAsync(cancellationToken))
        {
            return null;
        }

        var fileAnalysis = await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Sha256 == hash,
                cancellationToken)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Md5 == hash,
                cancellationToken)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Sha1 == hash,
                cancellationToken)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(
                f => f.ContentHashSet.Sha512 == hash,
                cancellationToken);

        return fileAnalysis;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(FileMultiAnalysis multiAnalysis, CancellationToken cancellationToken = default)
    {
        bool analysisExists = await _dbContext.FileMultiAnalyses.AnyAsync(
            f => f.Id == multiAnalysis.Id, cancellationToken);

        if (!analysisExists)
        {
            return;
        }

        _dbContext.FileMultiAnalyses.Update(multiAnalysis);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(FileMultiAnalysisId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileMultiAnalyses.AnyAsync(f => f.Id == id, cancellationToken);
    }
}