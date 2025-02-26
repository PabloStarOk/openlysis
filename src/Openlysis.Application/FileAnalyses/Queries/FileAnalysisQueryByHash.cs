using MediatR;

using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Query to retrieve many multi analysis of a file identified by hash.
/// </summary>
/// <param name="Hash">A <see cref="string"/> representing a MD5, SHA1, SHA256, or SHA512 hash of the file.</param>
/// <param name="FileAnalysesAmount">An <see cref="int"/> representing the number of <see cref="FileMultiAnalysis"/> to retrieve.</param>
/// <param name="StartedDateOrder">A <see cref="string"/> representing the order of <see cref="FileMultiAnalysis.StartedDate"/>.</param>
public record FileAnalysisQueryByHash(
    string Hash,
    int FileAnalysesAmount,
    string StartedDateOrder)
    : IRequest<IEnumerable<FileMultiAnalysis>>;