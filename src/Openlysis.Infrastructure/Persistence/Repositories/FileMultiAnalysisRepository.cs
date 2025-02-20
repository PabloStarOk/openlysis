using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository to access <see cref="FileMultiAnalysis"/>.
/// </summary>
public class FileMultiAnalysisRepository : IFileMultiAnalysisRepository
{
    private readonly AnalysesDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to be used by the repository.</param>
    public FileMultiAnalysisRepository(AnalysesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task AddAsync(FileMultiAnalysis fileMultiAnalysis)
    {
        await _dbContext.AddAsync(fileMultiAnalysis.ContentHashSet);
        await _dbContext.SaveChangesAsync();

        _dbContext.Entry(fileMultiAnalysis).Property("Sha256").CurrentValue = fileMultiAnalysis.ContentHashSet.Sha256;
        await _dbContext.AddAsync(fileMultiAnalysis);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetByIdAsync(FileMultiAnalysisId fileMultiAnalysisId)
    {
        return await _dbContext.FileMultiAnalyses.FindAsync(fileMultiAnalysisId);
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetByHashAsync(ContentHashSet contentHashSet)
    {
        if (!await _dbContext.FileMultiAnalyses.AnyAsync())
        {
            return null;
        }

        var fileAnalysis = await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Sha256 == contentHashSet.Md5)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Md5 == contentHashSet.Sha1)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Sha1 == contentHashSet.Sha256)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Sha512 == contentHashSet.Sha512);

        return fileAnalysis;
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetByHashAsync(string hash)
    {
        if (!await _dbContext.FileMultiAnalyses.AnyAsync())
        {
            return null;
        }

        var fileAnalysis = await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Sha256 == hash)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Md5 == hash)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Sha1 == hash)
            ?? await _dbContext.FileMultiAnalyses.FirstOrDefaultAsync(f =>
                f.ContentHashSet.Sha512 == hash);

        return fileAnalysis;
    }
}