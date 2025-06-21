using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="EmailAddressMultiReputation"/> entities.
/// </summary>
public class EmailAddressMultiReputationRepository
    : IRepository<EmailAddressMultiReputation, GlobalId>
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressMultiReputationRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to be used by the repository.</param>
    public EmailAddressMultiReputationRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<EmailAddressMultiReputation?> GetAsync(
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailAddressMultiReputations
            .AsNoTracking()
            .AsSplitQuery()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmailAddressMultiReputation>> GetManyAsync(
        int amount = 10,
        Expression<Func<EmailAddressMultiReputation, bool>>? filter = null,
        Func<IQueryable<EmailAddressMultiReputation>, IOrderedQueryable<EmailAddressMultiReputation>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<EmailAddressMultiReputation> query = _dbContext
            .EmailAddressMultiReputations
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
    public async Task<IReadOnlyList<EmailAddressMultiReputation>> GetManyByIdsAsync(
        GlobalId[] ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        return await _dbContext.EmailAddressMultiReputations
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        EmailAddressMultiReputation model,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailAddressMultiReputations
            .AddAsync(model, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(
        EmailAddressMultiReputation model,
        CancellationToken cancellationToken = default)
    {
        bool exists = await ExistsAsync(model.Id, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException("Given EmailAddressMultiReputation doesn't exist in the database.");
        }

        _dbContext.EmailAddressMultiReputations.Update(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if an <see cref="EmailAddressMultiReputation"/> entity with the specified <paramref name="id"/> exists in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to check for existence.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    private async Task<bool> ExistsAsync(
        GlobalId id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailAddressMultiReputations
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
}