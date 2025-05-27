using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;

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

        EntityEntry<FileMultiAnalysis> multiAnalysisEntry = await _dbContext.AddAsync(fileMultiAnalysis, cancellationToken);
        await SyncServiceAnalysesAsync(multiAnalysisEntry, cancellationToken);

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
        IQueryable<FileMultiAnalysis> query = _dbContext.FileMultiAnalyses
            .AsNoTracking()
            .AsSplitQuery();

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
        await SyncServiceAnalysesAsync(multiAnalysisEntry, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(GlobalId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FileMultiAnalyses
            .AsNoTracking()
            .AnyAsync(f => f.Id == id, cancellationToken);
    }

    /// <summary>
    /// Synchronizes the service analyses of a given <see cref="FileMultiAnalysis"/> entity with the database.
    /// </summary>
    /// <param name="multiAnalysisEntry">The entity entry of the <see cref="FileMultiAnalysis"/> to synchronize.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task SyncServiceAnalysesAsync(
        EntityEntry<FileMultiAnalysis> multiAnalysisEntry,
        CancellationToken cancellationToken = default)
    {
        var existingAnalyses = _dbContext.FileAnalyses
            .AsSplitQuery()
            .Where(f => multiAnalysisEntry.Entity.Analyses.Contains(f));

        foreach (var incomingAnalysis in multiAnalysisEntry.Entity.Analyses)
        {
            var existingAnalysis = await existingAnalyses
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e == incomingAnalysis, cancellationToken);

            if (existingAnalysis is null)
            {
                continue;
            }

            EntityEntry<FileAnalysis> incomingEntry = _dbContext.Entry(incomingAnalysis);
            incomingEntry.State = incomingAnalysis.HasSameStateTo(existingAnalysis)
                    ? EntityState.Unchanged
                    : EntityState.Modified;

            SyncReportsAsync(incomingEntry, existingAnalysis.Reports.ToArray());
        }
    }

    /// <summary>
    /// Synchronizes the reports of a given <see cref="FileAnalysis"/> entity with the database.
    /// </summary>
    /// <param name="serviceAnalysisEntry">The entity entry of the <see cref="FileAnalysis"/> to synchronize.</param>
    /// <param name="existingReports">The array of existing reports in the database to compare against.</param>
    private void SyncReportsAsync(
        EntityEntry<FileAnalysis> serviceAnalysisEntry,
        FileReport[] existingReports)
    {
        foreach (FileReport incomingReport in serviceAnalysisEntry.Entity.Reports)
        {
            var existingReport = existingReports.SingleOrDefault(
                r => r == incomingReport);

            if (existingReport is null)
            {
                continue;
            }

            _dbContext.Entry(incomingReport).State =
                incomingReport.HasSameStateTo(existingReport)
                ? EntityState.Unchanged
                : EntityState.Modified;
        }
    }
}