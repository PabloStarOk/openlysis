using MediatR;

using Openlysis.Application.Common.Enums;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.URLs.Queries;

/// <summary>
/// Query to retrieve a collection of <see cref="UrlMultiAnalysis"/> by hash.
/// </summary>
/// <param name="Hash">The hash of the URL.</param>
/// <param name="UserId">The ID of the user requesting the analyses.</param>
/// <param name="Amount">The number of analyses to retrieve. Default is 10.</param>
/// <param name="StartedDateOrder">The order type for sorting by start date. Default is descending.</param>
public record UrlMultiAnalysesByHashQuery(
    string Hash,
    UserId UserId,
    int Amount = 10,
    OrderType StartedDateOrder = OrderType.Dsc)
    : IRequest<IReadOnlyList<UrlMultiAnalysis>>;