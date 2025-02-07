using ErrorOr;

using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Application.FileAnalyses.Ports;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Handles <see cref="FileAnalysisQueryById"/>.
/// </summary>
public class FileAnalysisQueryByIdHandler
    : IRequestHandler<FileAnalysisQueryById, ErrorOr<FileAnalysis>>
{
    private readonly IFileAnalysisRepository _fileAnalysisRepository;
    private readonly IEnumerable<IFileAnalyzer> _fileAnalyzers;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAnalysisQueryByIdHandler"/> class.
    /// </summary>
    /// <param name="fileAnalysisRepository">Repository of file analyses.</param>
    /// <param name="fileAnalyzers">An <see cref="IEnumerable{T}"/> of <see cref="IFileAnalyzer"/>.</param>
    public FileAnalysisQueryByIdHandler(
        IFileAnalysisRepository fileAnalysisRepository,
        IEnumerable<IFileAnalyzer> fileAnalyzers)
    {
        _fileAnalysisRepository = fileAnalysisRepository;
        _fileAnalyzers = fileAnalyzers;
    }

    /// <summary>
    /// Handles a <see cref="FileAnalysisQueryById"/>.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ErrorOr"/> in case of an error, or a <see cref="FileAnalysis"/>.</returns>
    public async Task<ErrorOr<FileAnalysis>> Handle(FileAnalysisQueryById query, CancellationToken cancellationToken)
    {
        var fileAnalysis = await _fileAnalysisRepository.GetByIdAsync(query.FileAnalysisId);

        if (fileAnalysis is null)
        {
            return Error.NotFound();
        }

        var tasks = _fileAnalyzers.Select(f =>
            f.GetReportsAsync(fileAnalysis.Id, cancellationToken));

        foreach (var result in await Task.WhenAll(tasks))
        {
            fileAnalysis.AddReports(result);
        }

        return fileAnalysis;
    }
}