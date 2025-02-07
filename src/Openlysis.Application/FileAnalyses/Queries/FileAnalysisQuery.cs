using ErrorOr;

using MediatR;

using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Query to retrieve a file analysis.
/// </summary>
/// <param name="FileAnalysisId">A <see cref="FileAnalysisId"/>.</param>
public record FileAnalysisQuery(FileAnalysisId FileAnalysisId)
    : IRequest<ErrorOr<FileAnalysis>>;