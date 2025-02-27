using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository to access <see cref="FileMultiAnalysis"/>.
/// </summary>
public class FileMultiAnalysisRepository : IRepository<FileMultiAnalysis, FileMultiAnalysisId>
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
        return await _dbContext.FileMultiAnalyses
            .Include(f => f.ContentHashSet)
            .FirstOrDefaultAsync(f => f.Id == fileMultiAnalysisId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FileMultiAnalysis>> GetManyAsync(
        int amount = 10,
        Expression<Func<FileMultiAnalysis, bool>>? filter = null,
        Func<IQueryable<FileMultiAnalysis>, IOrderedQueryable<FileMultiAnalysis>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<FileMultiAnalysis> query = _dbContext.FileMultiAnalyses
            .Include(f => f.ContentHashSet);

        if (filter is not null)
        {
            query = query
                .Where(filter);
        }

        if (orderBy is not null)
        {
            query = orderBy(query)
                .Take(amount);
        }

        return await query.ToArrayAsync(cancellationToken);
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