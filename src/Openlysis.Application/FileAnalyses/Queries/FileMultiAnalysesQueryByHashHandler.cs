using MediatR;

using Openlysis.Application.Common.Interfaces.Persistence;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.Application.FileAnalyses.Queries;

/// <summary>
/// Handles <see cref="FileMultiAnalysesQueryByHash"/>.
/// </summary>
public class FileMultiAnalysesQueryByHashHandler : IRequestHandler<FileMultiAnalysesQueryByHash, IEnumerable<FileMultiAnalysis>>
{
    private readonly IRepository<FileMultiAnalysis, FileMultiAnalysisId> _fileMultiAnalysisRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysesQueryByHashHandler"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    public FileMultiAnalysesQueryByHashHandler(
        IRepository<FileMultiAnalysis, FileMultiAnalysisId> fileMultiAnalysisRepository)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
    }

    /// <summary>
    /// Handles a <see cref="FileMultiAnalysesQueryByHash"/>.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="IEnumerable{FileMultiAnalysis}"/>.</returns>
    public async Task<IEnumerable<FileMultiAnalysis>> Handle(FileMultiAnalysesQueryByHash query, CancellationToken cancellationToken)
    {
        IEnumerable<FileMultiAnalysis> fileAnalyses = await _fileMultiAnalysisRepository.GetManyAsync(
            query.FileAnalysesAmount,
            f => f.ContentHashSet.Sha256 == query.Hash
                || f.ContentHashSet.Md5 == query.Hash
                || f.ContentHashSet.Sha1 == query.Hash
                || f.ContentHashSet.Sha512 == query.Hash,
            cancellationToken: cancellationToken);

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