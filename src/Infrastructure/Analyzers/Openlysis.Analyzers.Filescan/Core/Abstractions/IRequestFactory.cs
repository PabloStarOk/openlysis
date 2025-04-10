using Openlysis.Analyzers.Filescan.Core.Models.Objects;

namespace Openlysis.Analyzers.Filescan.Core.Abstractions;

/// <summary>
/// Defines a base factory to create <see cref="FilescanAnalysisRequest"/>.
/// </summary>
public interface IRequestFactory
{
    /// <summary>
    /// Creates a new instance of FilescanAnalysisRequest.
    /// </summary>
    /// <returns>A new <see cref="FilescanAnalysisRequest"/> object.</returns>
    public FilescanAnalysisRequest Create();
}