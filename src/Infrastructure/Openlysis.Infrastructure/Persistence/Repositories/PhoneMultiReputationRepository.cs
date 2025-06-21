using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="PhoneMultiReputation"/> entities.
/// </summary>
public class PhoneMultiReputationRepository : IRepository<PhoneMultiReputation, GlobalId>
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneMultiReputationRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to be used by the repository.</param>
    public PhoneMultiReputationRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<PhoneMultiReputation?> GetAsync(
        GlobalId globalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PhoneMultiReputations
            .AsNoTracking()
            .AsSplitQuery()
            .SingleOrDefaultAsync(p => p.Id == globalId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PhoneMultiReputation>> GetManyAsync(
        int amount = 10,
        Expression<Func<PhoneMultiReputation, bool>>? filter = null,
        Func<IQueryable<PhoneMultiReputation>, IOrderedQueryable<PhoneMultiReputation>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PhoneMultiReputation> query = _dbContext.PhoneMultiReputations
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
    public async Task<IReadOnlyList<PhoneMultiReputation>> GetManyByIdsAsync(
        GlobalId[] ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        return await _dbContext.PhoneMultiReputations
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PhoneMultiReputation model, CancellationToken cancellationToken = default)
    {
        await _dbContext.PhoneMultiReputations.AddAsync(model, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PhoneMultiReputation model, CancellationToken cancellationToken = default)
    {
        bool exists = await ExistsAsync(model.Id, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException("Given PhoneMultiReputation doesn't exist in the database.");
        }

        _dbContext.PhoneMultiReputations.Update(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a <see cref="PhoneMultiReputation"/> entity with the specified <paramref name="globalId"/> exists in the database.
    /// </summary>
    /// <param name="globalId">The global identifier of the entity to check for existence.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    private async Task<bool> ExistsAsync(
        GlobalId globalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PhoneMultiReputations
            .AsNoTracking()
            .AnyAsync(p => p.Id == globalId, cancellationToken);
    }
}