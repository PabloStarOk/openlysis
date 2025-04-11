using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

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
                c => c.Sha256 == fileMultiAnalysis.DataHashSet.Sha256,
                cancellationToken);

        if (hashSetExists)
        {
            _dbContext.ChangeTracker.Clear();
            _dbContext.Attach(fileMultiAnalysis.DataHashSet).State = EntityState.Unchanged;
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
        int amount = 10,
        Expression<Func<FileMultiAnalysis, bool>>? filter = null,
        Func<IQueryable<FileMultiAnalysis>, IOrderedQueryable<FileMultiAnalysis>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<FileMultiAnalysis> query = _dbContext.FileMultiAnalyses.AsSplitQuery();

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

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(FileMultiAnalysis model, CancellationToken cancellationToken = default)
    {
        bool analysisExists = await _dbContext.FileMultiAnalyses
            .AsSplitQuery()
            .AnyAsync(f => f.Id == model.Id, cancellationToken);

        if (!analysisExists)
        {
            return;
        }

        _dbContext.FileMultiAnalyses.Update(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(GlobalId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileMultiAnalyses.AnyAsync(f => f.Id == id, cancellationToken);
    }
}