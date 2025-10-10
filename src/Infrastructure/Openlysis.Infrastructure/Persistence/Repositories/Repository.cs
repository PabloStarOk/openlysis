using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation for entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type, must inherit from <see cref="Entity{GlobalId}"/>.</typeparam>
internal class Repository<TEntity> : IRepository<TEntity>
    where TEntity : Entity<GlobalId>
{
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/> used for data access.</param>
    public Repository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    /// <inheritdoc/>
    public async Task<TEntity?> GetAsync(GlobalId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsSplitQuery().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsSplitQuery();

        query = query.Where(filter);
        query = orderBy is not null
            ? orderBy(query)
            : query.OrderByDescending(x => x.Id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TEntity>> GetManyAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsSplitQuery();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        query = orderBy is not null
            ? orderBy(query)
            : query.OrderByDescending(x => x.Id);

        int skippablePages = Math.Max(0, page - 1);
        int skippableEntities = skippablePages * pageSize;
        return await query
            .Skip(skippableEntities)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TEntity>> GetManyByIdsAsync(
        GlobalId[] ids,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsSplitQuery()
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public void Update(TEntity entity)
    {
        _dbSet.Attach(entity);
        _dbSet.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc/>
    public async Task SaveChangeAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}