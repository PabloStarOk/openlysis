using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository to access <see cref="FileMultiAnalysis"/>.
/// </summary>
public class FileMultiAnalysisRepository : IRepository<FileMultiAnalysis, GlobalId>
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to be used by the repository.</param>
    public FileMultiAnalysisRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task AddAsync(FileMultiAnalysis fileMultiAnalysis, CancellationToken cancellationToken = default)
    {
        var hashSetExists = await _dbContext.ContentHashSets
            .AnyAsync(
                c => c.Sha256 == fileMultiAnalysis.DataHashValues.Sha256,
                cancellationToken);

        if (hashSetExists)
        {
            _dbContext.ChangeTracker.Clear();
            _dbContext.Attach(fileMultiAnalysis.DataHashValues).State = EntityState.Unchanged;
        }

        await _dbContext.AddAsync(fileMultiAnalysis, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetAsync(GlobalId fileMultiAnalysisId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileMultiAnalyses
            .AsSplitQuery()
            .FirstOrDefaultAsync(f => f.Id == fileMultiAnalysisId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileMultiAnalysis>> GetManyAsync(
        int page,
        int pageSize,
        Expression<Func<FileMultiAnalysis, bool>>? filter = null,
        Func<IQueryable<FileMultiAnalysis>, IOrderedQueryable<FileMultiAnalysis>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<FileMultiAnalysis> query = _dbContext.FileMultiAnalyses
            .AsNoTracking()
            .AsSplitQuery();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        query = orderBy is not null
            ? orderBy(query)
            : query.OrderByDescending(x => x.StartedDate);

        int skippablePages = Math.Max(0, page - 1);
        int skippableEntities = skippablePages * pageSize;
        return await query
            .Skip(skippableEntities)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileMultiAnalysis>> GetManyByIdsAsync(
        GlobalId[] ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        return await _dbContext.FileMultiAnalyses
            .AsNoTracking()
            .AsSplitQuery()
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(FileMultiAnalysis model, CancellationToken cancellationToken = default)
    {
        var multiAnalysis = await _dbContext.FileMultiAnalyses
            .AsSplitQuery()
            .Include(u => u.Analyses)
            .FirstOrDefaultAsync(m => m == model, cancellationToken);

        if (multiAnalysis is null)
        {
            throw new InvalidOperationException("UrlMultiAnalysis not found.");
        }

        EntityEntry<FileMultiAnalysis> multiAnalysisEntry = _dbContext.Entry(multiAnalysis);
        multiAnalysisEntry.CurrentValues.SetValues(model);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}