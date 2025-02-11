using Microsoft.EntityFrameworkCore;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository to access <see cref="FileAnalysis"/>.
/// </summary>
public class FileAnalysisRepository : IFileAnalysisRepository
{
    private readonly AnalysesDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to be used by the repository.</param>
    public FileAnalysisRepository(AnalysesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task AddAsync(FileAnalysis fileAnalysis)
    {
        await _dbContext.FileAnalyses.AddAsync(fileAnalysis);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<FileAnalysis?> GetByIdAsync(FileAnalysisId fileAnalysisId)
    {
        return await _dbContext.FileAnalyses.FindAsync(fileAnalysisId);
    }

    /// <inheritdoc/>
    public async Task<FileAnalysis?> GetByHashAsync(ContentHashSet contentHashSet)
    {
        var fileAnalysis = await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Sha256 == contentHashSet.Md5)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Md5 == contentHashSet.Sha1)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Sha1 == contentHashSet.Sha256)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Sha512 == contentHashSet.Sha512);

        return fileAnalysis;
    }

    /// <inheritdoc/>
    public async Task<FileAnalysis?> GetByHashAsync(string hash)
    {
        var fileAnalysis = await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Sha256 == hash)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Md5 == hash)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Sha1 == hash)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f =>
                f.Metadata.ContentHashSet.Sha512 == hash);

        return fileAnalysis;
    }
}