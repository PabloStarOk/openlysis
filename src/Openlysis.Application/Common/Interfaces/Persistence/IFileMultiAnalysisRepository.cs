using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines a repository for file analyses.
/// </summary>
public interface IFileMultiAnalysisRepository : IRepository<FileMultiAnalysis, FileMultiAnalysisId>
{
    /// <summary>
    /// Gets a file analysis from the repository by a <see cref="ContentHashSet"/>.
    /// </summary>
    /// <param name="contentHashSet">A <see cref="ContentHashSet"/>.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="FileMultiAnalysis"/>.</returns>
    public Task<FileMultiAnalysis?> GetByHashAsync(ContentHashSet contentHashSet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a file analysis from the repository by its hash.
    /// </summary>
    /// <param name="hash">A <see cref="string"/> representing a MD5, SHA1, SHA256, or SHA512 hash of the file.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="FileMultiAnalysis"/>.</returns>
    public Task<FileMultiAnalysis?> GetByHashAsync(string hash, CancellationToken cancellationToken = default);
}