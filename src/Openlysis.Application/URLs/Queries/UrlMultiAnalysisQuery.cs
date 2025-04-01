using ErrorOr;

using MediatR;

using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.URLs.Queries;

/// <summary>
/// Represents a query to retrieve a <see cref="UrlMultiAnalysis"/> by ID.
/// </summary>
/// <param name="Id">The ID of the multi-analysis.</param>
/// <param name="UserId">The ID of the user requesting the analysis.</param>
public record UrlMultiAnalysisQuery(
    MultiAnalysisId Id,
    UserId UserId)
    : IRequest<ErrorOr<UrlMultiAnalysis>>;