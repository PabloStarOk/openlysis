using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.Application.Files.Queries;

/// <summary>
/// Handles <see cref="FileMultiAnalysisQuery"/>.
/// </summary>
public class FileMultiAnalysisQueryHandler
    : IRequestHandler<FileMultiAnalysisQuery, ErrorOr<FileMultiAnalysis>>
{
    private readonly IRepository<FileMultiAnalysis, GlobalId> _fileMultiAnalysisRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisQueryHandler"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    public FileMultiAnalysisQueryHandler(
        IRepository<FileMultiAnalysis, GlobalId> fileMultiAnalysisRepository)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
    }

    /// <summary>
    /// Handles a <see cref="FileMultiAnalysisQuery"/>.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ErrorOr"/> in case of an error, or a <see cref="FileMultiAnalysis"/>.</returns>
    public async Task<ErrorOr<FileMultiAnalysis>> Handle(FileMultiAnalysisQuery query, CancellationToken cancellationToken)
    {
        var fileAnalysis = await _fileMultiAnalysisRepository.GetAsync(query.FileMultiAnalysisId, cancellationToken);

        if (fileAnalysis is null)
        {
            return Error.NotFound();
        }

        return fileAnalysis;
    }
}