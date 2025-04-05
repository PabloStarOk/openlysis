using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Phones;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing PhoneMultiValidation entities.
/// </summary>
public class PhoneMultiValidationRepository : IRepository<PhoneMultiValidation, Id>
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneMultiValidationRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to be used by the repository.</param>
    public PhoneMultiValidationRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<PhoneMultiValidation?> GetAsync(
        Id id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PhoneMultiValidations
            .AsSplitQuery()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PhoneMultiValidation>> GetManyAsync(
        int amount = 10,
        Expression<Func<PhoneMultiValidation, bool>>? filter = null,
        Func<IQueryable<PhoneMultiValidation>, IOrderedQueryable<PhoneMultiValidation>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PhoneMultiValidation> query = _dbContext.PhoneMultiValidations
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
    public async Task AddAsync(PhoneMultiValidation model, CancellationToken cancellationToken = default)
    {
        await _dbContext.PhoneMultiValidations.AddAsync(model, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PhoneMultiValidation model, CancellationToken cancellationToken = default)
    {
        bool exists = await ExistsAsync(model.Id, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException("Given PhoneMultiValidation doesn't exist in the database.");
        }

        _dbContext.PhoneMultiValidations.Update(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(Id id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PhoneMultiValidations
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
}