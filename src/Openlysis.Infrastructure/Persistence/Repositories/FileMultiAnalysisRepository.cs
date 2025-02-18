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
        await _dbContext.FileAnalyses.AddAsync(fileMultiAnalysis);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetByIdAsync(FileMultiAnalysisId fileMultiAnalysisId)
    {
        return await _dbContext.FileAnalyses.FindAsync(fileMultiAnalysisId);
    }

    /// <inheritdoc/>
    public async Task<FileMultiAnalysis?> GetByHashAsync(ContentHashSet contentHashSet)
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
    public async Task<FileMultiAnalysis?> GetByHashAsync(string hash)
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