using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Handles <see cref="FileAnalysisQueryById"/>.
/// </summary>
public class FileAnalysisQueryByIdHandler
    : IRequestHandler<FileAnalysisQueryById, ErrorOr<FileMultiAnalysis>>
{
    private readonly IFileMultiAnalysisRepository _fileMultiAnalysisRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisQueryByIdHandler"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    public FileAnalysisQueryByIdHandler(
        IFileMultiAnalysisRepository fileMultiAnalysisRepository)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
    }

    /// <summary>
    /// Handles a <see cref="FileAnalysisQueryById"/>.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ErrorOr"/> in case of an error, or a <see cref="FileMultiAnalysis"/>.</returns>
    public async Task<ErrorOr<FileMultiAnalysis>> Handle(FileAnalysisQueryById query, CancellationToken cancellationToken)
    {
        var fileAnalysis = await _fileMultiAnalysisRepository.GetAsync(query.FileMultiAnalysisId, cancellationToken);

        if (fileAnalysis is null)
        {
            return Error.NotFound();
        }

        return fileAnalysis;
    }
}