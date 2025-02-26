using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Handles <see cref="FileAnalysisQueryByHash"/>.
/// </summary>
public class FileAnalysisQueryByHashHandler : IRequestHandler<FileAnalysisQueryByHash, IEnumerable<FileMultiAnalysis>>
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
    /// Handles a <see cref="FileAnalysisQueryByHash"/>.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="IEnumerable{FileMultiAnalysis}"/>.</returns>
    public async Task<IEnumerable<FileMultiAnalysis>> Handle(FileAnalysisQueryByHash query, CancellationToken cancellationToken)
    {
        IEnumerable<FileMultiAnalysis> fileAnalyses = await _fileMultiAnalysisRepository.GetManyAsync(
            f => f.ContentHashSet.Sha256 == query.Hash
                || f.ContentHashSet.Md5 == query.Hash
                || f.ContentHashSet.Sha1 == query.Hash
                || f.ContentHashSet.Sha512 == query.Hash,
            query.FileAnalysesAmount,
            cancellationToken);

        string dateOrder = query.StartedDateOrder.Trim().ToLower();

        fileAnalyses = dateOrder switch
        {
            "asc" => fileAnalyses.OrderBy(f => f.StartedDate),
            "dsc" => fileAnalyses.OrderByDescending(f => f.StartedDate),
            _ => fileAnalyses
        };

        return fileAnalyses;
    }
}