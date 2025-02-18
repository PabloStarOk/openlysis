using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.FileAnalyses.Ports;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Handles <see cref="FileAnalysisQueryByHash"/>.
/// </summary>
public class FileAnalysisQueryByHashHandler : IRequestHandler<FileAnalysisQueryByHash, ErrorOr<FileMultiAnalysis>>
{
    private readonly IFileMultiAnalysisRepository _fileMultiAnalysisRepository;
    private readonly IEnumerable<IFileAnalyzer> _fileAnalyzers;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisQueryByHashHandler"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    /// <param name="fileAnalyzers">An <see cref="IEnumerable{T}"/> of <see cref="IFileAnalyzer"/>.</param>
    public FileAnalysisQueryByHashHandler(
        IFileMultiAnalysisRepository fileMultiAnalysisRepository,
        IEnumerable<IFileAnalyzer> fileAnalyzers)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
        _fileAnalyzers = fileAnalyzers;
    }

    /// <summary>
    /// Handles a <see cref="FileAnalysisQueryById"/>.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ErrorOr"/> in case of an error, or a <see cref="FileMultiAnalysis"/>.</returns>
    public async Task<ErrorOr<FileMultiAnalysis>> Handle(FileAnalysisQueryByHash query, CancellationToken cancellationToken)
    {
        var fileAnalysis = await _fileMultiAnalysisRepository.GetByHashAsync(query.Hash);

        if (fileAnalysis is null)
        {
            return Error.NotFound();
        }

        return fileAnalysis;
    }
}