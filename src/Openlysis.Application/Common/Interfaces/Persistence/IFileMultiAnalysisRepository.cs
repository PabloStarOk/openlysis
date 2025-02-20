using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines a repository for file analyses.
/// </summary>
public interface IFileMultiAnalysisRepository
{
    /// <summary>
    /// Adds a file analysis to the repository.
    /// </summary>
    /// <param name="fileMultiAnalysis">File analysis to save.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddAsync(FileMultiAnalysis fileMultiAnalysis);

    /// <summary>
    /// Gets a file analysis from the repository by its ID.
    /// </summary>
    /// <param name="fileMultiAnalysisId">ID of the file analysis to get.</param>
    /// <returns>A <see cref="FileMultiAnalysis"/>.</returns>
    public Task<FileMultiAnalysis?> GetByIdAsync(FileMultiAnalysisId fileMultiAnalysisId);

    /// <summary>
    /// Gets a file analysis from the repository by a <see cref="ContentHashSet"/>.
    /// </summary>
    /// <param name="contentHashSet">A <see cref="ContentHashSet"/>.</param>
    /// <returns>A <see cref="FileMultiAnalysis"/>.</returns>
    public Task<FileMultiAnalysis?> GetByHashAsync(ContentHashSet contentHashSet);

    /// <summary>
    /// Gets a file analysis from the repository by its hash.
    /// </summary>
    /// <param name="hash">A <see cref="string"/> representing a MD5, SHA1, SHA256, or SHA512 hash of the file.</param>
    /// <returns>A <see cref="FileMultiAnalysis"/>.</returns>
    public Task<FileMultiAnalysis?> GetByHashAsync(string hash);
}