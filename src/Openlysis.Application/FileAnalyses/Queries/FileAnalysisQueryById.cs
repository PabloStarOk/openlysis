using ErrorOr;

using MediatR;

using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Query to retrieve a file analysis by its id.
/// </summary>
/// <param name="FileMultiAnalysisId">A <see cref="FileMultiAnalysisId"/>.</param>
public record FileAnalysisQueryById(FileMultiAnalysisId FileMultiAnalysisId)
    : IRequest<ErrorOr<FileMultiAnalysis>>;