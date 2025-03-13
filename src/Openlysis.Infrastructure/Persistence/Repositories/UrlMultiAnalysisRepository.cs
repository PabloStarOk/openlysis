using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.URLs;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="UrlMultiAnalysis"/> entities.
/// </summary>
/// <typeparam name="UrlMultiAnalysis">The type of the entity.</typeparam>
/// <typeparam name="MultiAnalysisId">The type of the entity's identifier.</typeparam>
public class UrlMultiAnalysisRepository : IRepository<UrlMultiAnalysis, MultiAnalysisId>
{
    private readonly AnalysesDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysisRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for accessing the data store.</param>
    public UrlMultiAnalysisRepository(AnalysesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<UrlMultiAnalysis?> GetAsync(
        MultiAnalysisId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _dbContext.UrlMultiAnalyses
            .AsSplitQuery()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetManyAsync(
        int amount = 10,
        Expression<Func<UrlMultiAnalysis, bool>>? filter = null,
        Func<IQueryable<UrlMultiAnalysis>, IOrderedQueryable<UrlMultiAnalysis>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UrlMultiAnalysis> query = _dbContext.UrlMultiAnalyses;

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        return await query
            .Take(amount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(UrlMultiAnalysis model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        bool hashExists = await _dbContext.ContentHashSets
            .AnyAsync(c => c.Sha256 == model.UrlHashSet.Sha256, cancellationToken);

        if (hashExists)
        {
            _dbContext.ChangeTracker.Clear();
            _dbContext.Attach(model.UrlHashSet).State = EntityState.Unchanged;
        }

        await _dbContext.AddAsync(model, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(UrlMultiAnalysis analysis, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        bool analysisExists = await _dbContext.UrlMultiAnalyses
            .AsSplitQuery()
            .AnyAsync(u => u.Id == analysis.Id, cancellationToken);

        if (!analysisExists)
        {
            return;
        }

        _dbContext.UrlMultiAnalyses.Update(analysis);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(MultiAnalysisId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UrlMultiAnalyses.AnyAsync(u => u.Id == id, cancellationToken);
    }
}