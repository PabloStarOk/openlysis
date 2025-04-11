using ErrorOr;

using MediatR;

using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.Application.Files.Queries;

/// <summary>
/// Query to retrieve a file analysis by its id.
/// </summary>
/// <param name="FileMultiAnalysisId">A <see cref="FileMultiAnalysisId"/>.</param>
public record FileMultiAnalysisQuery(GlobalId FileMultiAnalysisId)
    : IRequest<ErrorOr<FileMultiAnalysis>>;