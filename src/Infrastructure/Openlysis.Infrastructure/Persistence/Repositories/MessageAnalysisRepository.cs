using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing <see cref="MessageAnalysis"/> entities.
/// </summary>
public class MessageAnalysisRepository : IRepository<MessageAnalysis, GlobalId>
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used to access the data store.</param>
    public MessageAnalysisRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<MessageAnalysis?> GetAsync(
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _dbContext.MessageAnalyses
            .AsNoTracking()
            .AsSplitQuery()
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MessageAnalysis>> GetManyAsync(
        int amount = 10,
        Expression<Func<MessageAnalysis, bool>>? filter = null,
        Func<IQueryable<MessageAnalysis>, IOrderedQueryable<MessageAnalysis>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MessageAnalysis> query = _dbContext.MessageAnalyses
            .AsNoTracking()
            .AsSplitQuery();

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
    public async Task<IReadOnlyList<MessageAnalysis>> GetManyByIdsAsync(
        GlobalId[] ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        return await _dbContext.MessageAnalyses
            .AsNoTracking()
            .AsSplitQuery()
            .Where(m => ids.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        MessageAnalysis model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var hashSetExists = await _dbContext.ContentHashSets
            .AnyAsync(
                c => c.Sha256 == model.Message.MessageHashSet.Sha256,
                cancellationToken);

        if (hashSetExists)
        {
            _dbContext.ChangeTracker.Clear();
            _dbContext.Attach(model.Message.MessageHashSet).State = EntityState.Unchanged;
        }

        await _dbContext.MessageAnalyses.AddAsync(model, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(
        MessageAnalysis model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        bool exists = await ExistsAsync(model.Id, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException("Given MessageAnalysis doesn't exist in the database.");
        }

        _dbContext.MessageAnalyses.Update(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.MessageAnalyses
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
}