using System.Text;

using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.Common.Interfaces.Services;
using Openlysis.Domain.Common.Hash;
using Openlysis.Domain.Common.MultiAnalyses.ValueObjects;
using Openlysis.Domain.URLs;

namespace Openlysis.Application.URLs.Commands;

/// <summary>
/// Handles the <see cref="AnalyzeUrlCommand"/> and returns an <see cref="ErrorOr{TValue}"/> containing <see cref="UrlMultiAnalysis"/>.
/// </summary>
public class AnalyzeUrlCommandHandler : IRequestHandler<AnalyzeUrlCommand, ErrorOr<UrlMultiAnalysis>>
{
    private readonly TimeProvider _timeProvider;
    private readonly IHashService _hashService;
    private readonly IUrlMultiAnalyzer _urlMultiAnalyzer;
    private readonly IRepository<UrlMultiAnalysis, MultiAnalysisId> _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlCommandHandler"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="hashService">The hash service.</param>
    /// <param name="urlMultiAnalyzer">The URL multi-analyzer.</param>
    /// <param name="repository">The repository for URL multi-analysis.</param>
    public AnalyzeUrlCommandHandler(
        TimeProvider timeProvider,
        IHashService hashService,
        IUrlMultiAnalyzer urlMultiAnalyzer,
        IRepository<UrlMultiAnalysis, MultiAnalysisId> repository)
    {
        _timeProvider = timeProvider;
        _hashService = hashService;
        _urlMultiAnalyzer = urlMultiAnalyzer;
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<ErrorOr<UrlMultiAnalysis>> Handle(
       AnalyzeUrlCommand request,
       CancellationToken cancellationToken)
    {
        ContentHashSet urlHashSet;
        byte[] urlBytes = Encoding.UTF8.GetBytes(request.Url.AbsoluteUri);
        await using (var urlMemoryStream = new MemoryStream(urlBytes))
        {
            urlHashSet = await _hashService.HashDataAsync(urlMemoryStream, cancellationToken);
        }

        var multiAnalysis = UrlMultiAnalysis.Create(
            4,
            request.UserId,
            request.IsPrivate,
            _timeProvider.GetUtcNow().UtcDateTime,
            request.Url,
            urlHashSet);

        await _urlMultiAnalyzer.StartAnalysisAsync(multiAnalysis.Id, request.Url, cancellationToken);
        await _repository.AddAsync(multiAnalysis, cancellationToken);
        return multiAnalysis;
    }
}