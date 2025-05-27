using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="UrlMultiAnalysis"/> entities.
/// </summary>
public class UrlMultiAnalysisRepository : IRepository<UrlMultiAnalysis, GlobalId>
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysisRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for accessing the data store.</param>
    public UrlMultiAnalysisRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<UrlMultiAnalysis?> GetAsync(
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _dbContext.UrlMultiAnalyses
            .AsSplitQuery()
            .Include(u => u.Analyses)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetManyAsync(
        int amount = 10,
        Expression<Func<UrlMultiAnalysis, bool>>? filter = null,
        Func<IQueryable<UrlMultiAnalysis>, IOrderedQueryable<UrlMultiAnalysis>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UrlMultiAnalysis> query = _dbContext.UrlMultiAnalyses
            .AsNoTracking()
            .Include(u => u.Analyses);

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
    public async Task<IReadOnlyList<UrlMultiAnalysis>> GetManyByIdsAsync(
        GlobalId[] ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        return await _dbContext.UrlMultiAnalyses
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.Analyses)
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(UrlMultiAnalysis model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        bool hashExists = await _dbContext.ContentHashSets
            .AnyAsync(c => c.Sha256 == model.DataHashValues.Sha256, cancellationToken);

        if (hashExists)
        {
            _dbContext.ChangeTracker.Clear();
            _dbContext.Attach(model.DataHashValues).State = EntityState.Unchanged;
        }

        EntityEntry<UrlMultiAnalysis> multiAnalysisEntry = await _dbContext.AddAsync(model, cancellationToken);
        await SyncServiceAnalysesAsync(multiAnalysisEntry, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(UrlMultiAnalysis model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var multiAnalysis = await _dbContext.UrlMultiAnalyses
            .Include(u => u.Analyses)
            .FirstOrDefaultAsync(m => m == model, cancellationToken);
        if (multiAnalysis is null)
        {
            throw new InvalidOperationException("UrlMultiAnalysis not found.");
        }

        EntityEntry<UrlMultiAnalysis> multiAnalysisEntry = _dbContext.Entry(multiAnalysis);
        multiAnalysisEntry.CurrentValues.SetValues(model);
        await SyncServiceAnalysesAsync(multiAnalysisEntry, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(GlobalId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UrlMultiAnalyses
            .AsNoTracking()
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// References existing service analyses and updates their state if necessary.
    /// </summary>
    /// <param name="multiAnalysisEntry">The entity entry of the UrlMultiAnalysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task SyncServiceAnalysesAsync(
        EntityEntry<UrlMultiAnalysis> multiAnalysisEntry,
        CancellationToken cancellationToken = default)
    {
        var existingAnalyses = _dbContext.UrlAnalyses
            .Where(u => multiAnalysisEntry.Entity.Analyses.Contains(u));

        foreach (var incomingAnalysis in multiAnalysisEntry.Entity.Analyses)
        {
            var existingAnalysis = await existingAnalyses
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e == incomingAnalysis, cancellationToken);

            if (existingAnalysis is null)
            {
                continue;
            }

            _dbContext.Entry(incomingAnalysis).State =
                incomingAnalysis.HasSameStateTo(existingAnalysis)
                ? EntityState.Unchanged
                : EntityState.Modified;
        }
    }
}