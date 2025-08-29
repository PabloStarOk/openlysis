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
        int page,
        int pageSize,
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
                c => c.Sha256 == model.Message.MessageHashValues.Sha256,
                cancellationToken);

        if (hashSetExists)
        {
            _dbContext.ChangeTracker.Clear();
            _dbContext.Attach(model.Message.MessageHashValues).State = EntityState.Unchanged;
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

        _dbContext.ChangeTracker.Clear();
        _dbContext.MessageAnalyses.Update(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a <see cref="MessageAnalysis"/> entity with the specified <paramref name="id"/> exists in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="MessageAnalysis"/> entity.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    private async Task<bool> ExistsAsync(
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _dbContext.MessageAnalyses
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
}