using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Handles <see cref="FileAnalysisQueryByHash"/>.
/// </summary>
public class FileAnalysisQueryByHashHandler : IRequestHandler<FileAnalysisQueryByHash, ErrorOr<FileMultiAnalysis>>
{
    private readonly IFileMultiAnalysisRepository _fileMultiAnalysisRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisQueryByHashHandler"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    public FileAnalysisQueryByHashHandler(
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
    public async Task<ErrorOr<FileMultiAnalysis>> Handle(FileAnalysisQueryByHash query, CancellationToken cancellationToken)
    {
        var fileAnalysis = await _fileMultiAnalysisRepository.GetByHashAsync(query.Hash, cancellationToken);

        if (fileAnalysis is null)
        {
            return Error.NotFound();
        }

        return fileAnalysis;
    }
}