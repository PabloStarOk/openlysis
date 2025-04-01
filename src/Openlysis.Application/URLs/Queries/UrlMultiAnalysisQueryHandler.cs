using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.URLs.Queries;

/// <summary>
/// Retrieves a <see cref="UrlMultiAnalysis"/> by <see cref="MultiAnalysisId"/> and <see cref="UserId"/>.
/// </summary>
/// <remarks>
/// This handler retrieves a URL multi-analysis by its ID and ensures the requesting user is authorized to access it.
/// </remarks>
public class UrlMultiAnalysisQueryHandler : IRequestHandler<UrlMultiAnalysisQuery, ErrorOr<UrlMultiAnalysis>>
{
    private readonly IRepository<UrlMultiAnalysis, MultiAnalysisId> _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalysisQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The repository to access URL multi-analysis data.</param>
    public UrlMultiAnalysisQueryHandler(
        IRepository<UrlMultiAnalysis, MultiAnalysisId> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<UrlMultiAnalysis>> Handle(UrlMultiAnalysisQuery request, CancellationToken cancellationToken)
    {
        UrlMultiAnalysis? multiAnalysis = await _repository.GetAsync(request.Id, cancellationToken);
        if (multiAnalysis is null)
        {
            return Error.NotFound();
        }

        if (multiAnalysis.UserId != request.UserId)
        {
            return Error.NotFound();
        }

        return multiAnalysis;
    }
}