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
    public async Task<FileAnalysis?> GetByHashAsync(HashSet hashSet)
    {
        var fileAnalysis = await _dbContext.FileAnalyses.FirstOrDefaultAsync(f => f.File.HashSet.Sha256 == hashSet.Sha256)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f => f.File.HashSet.Md5 == hashSet.Md5)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f => f.File.HashSet.Sha1 == hashSet.Sha1)
            ?? await _dbContext.FileAnalyses.FirstOrDefaultAsync(f => f.File.HashSet.Sha512 == hashSet.Sha512);

        return fileAnalysis;
    }
}