using ErrorOr;

using MediatR;

using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.Files.Queries;

/// <summary>
/// Query to retrieve a file analysis by its id.
/// </summary>
/// <param name="FileMultiAnalysisId">A <see cref="FileMultiAnalysisId"/>.</param>
public record FileMultiAnalysisQuery(FileMultiAnalysisId FileMultiAnalysisId)
    : IRequest<ErrorOr<FileMultiAnalysis>>;