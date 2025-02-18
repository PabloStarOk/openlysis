using ErrorOr;

using MediatR;

using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Query to retrieve a file analysis by its hash.
/// </summary>
/// <param name="Hash">A <see cref="string"/> representing a MD5, SHA1, SHA256, or SHA512 hash of the file.</param>
public record FileAnalysisQueryByHash(string Hash)
    : IRequest<ErrorOr<FileMultiAnalysis>>;