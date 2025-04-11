using MediatR;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;

namespace Openlysis.Application.URLs.Queries;

/// <summary>
/// Handles <see cref="UrlMultiAnalysesByHashQuery"/> queries.
/// </summary>
/// <remarks>
/// Implements the <see cref="IRequestHandler{TRequest, TResponse}"/> interface to handle
/// <see cref="UrlMultiAnalysesByHashQuery"/> requests and return a read-only list of <see cref="UrlMultiAnalysis"/>.
/// </remarks>
public class UrlMultiAnalysesByHashQueryHandler
    : IRequestHandler<UrlMultiAnalysesByHashQuery, IReadOnlyList<UrlMultiAnalysis>>
{
    private readonly IRepository<UrlMultiAnalysis, GlobalId> _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysesByHashQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The repository for accessing <see cref="UrlMultiAnalysis"/> entities.</param>
    public UrlMultiAnalysesByHashQueryHandler(
        IRepository<UrlMultiAnalysis, GlobalId> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UrlMultiAnalysis>> Handle(
        UrlMultiAnalysesByHashQuery request,
        CancellationToken cancellationToken)
    {
        var analyses = await _repository.GetManyAsync(
            request.Amount,
            u => (u.DataHashSet.Sha256 == request.Hash
            || u.DataHashSet.Md5 == request.Hash
            || u.DataHashSet.Sha1 == request.Hash
            || u.DataHashSet.Sha512 == request.Hash)
            && (!u.IsPrivate || (u.IsPrivate && u.UserId == request.UserId)),
            OrderBy,
            cancellationToken);

        return analyses.ToList().AsReadOnly();

        // Order
        IOrderedQueryable<UrlMultiAnalysis> OrderBy(IQueryable<UrlMultiAnalysis> q)
        {
            return request.StartedDateOrder switch
            {
                OrderType.Dsc => q.OrderByDescending(u => u.StartedDate),
                OrderType.Asc => q.OrderBy(u => u.StartedDate),
                _ => throw new InvalidOperationException("StartedDateOrder has an invalid enum value.")
            };
        }
    }
}