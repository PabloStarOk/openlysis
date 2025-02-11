using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines a repository for file analyses.
/// </summary>
public interface IFileAnalysisRepository
{
    /// <summary>
    /// Adds a file analysis to the repository.
    /// </summary>
    /// <param name="fileAnalysis">File analysis to save.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddAsync(FileAnalysis fileAnalysis);

    /// <summary>
    /// Gets a file analysis from the repository by its ID.
    /// </summary>
    /// <param name="fileAnalysisId">ID of the file analysis to get.</param>
    /// <returns>A <see cref="FileAnalysis"/>.</returns>
    public Task<FileAnalysis?> GetByIdAsync(FileAnalysisId fileAnalysisId);

    /// <summary>
    /// Gets a file analysis from the repository by a <see cref="HashSet"/>.
    /// </summary>
    /// <param name="hashSet">A <see cref="HashSet"/>.</param>
    /// <returns>A <see cref="FileAnalysis"/>.</returns>
    public Task<FileAnalysis?> GetByHashAsync(HashSet hashSet);

    /// <summary>
    /// Gets a file analysis from the repository by its hash.
    /// </summary>
    /// <param name="hash">A <see cref="string"/> representing a MD5, SHA1, SHA256, or SHA512 hash of the file.</param>
    /// <returns>A <see cref="FileAnalysis"/>.</returns>
    public Task<FileAnalysis?> GetByHashAsync(string hash);
}