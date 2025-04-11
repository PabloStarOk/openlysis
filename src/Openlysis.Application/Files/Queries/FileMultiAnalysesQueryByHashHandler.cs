using MediatR;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.Application.Files.Queries;

/// <summary>
/// Handles <see cref="FileMultiAnalysesQueryByHash"/>.
/// </summary>
public class FileMultiAnalysesQueryByHashHandler : IRequestHandler<FileMultiAnalysesQueryByHash, IReadOnlyList<FileMultiAnalysis>>
{
    private readonly IRepository<FileMultiAnalysis, GlobalId> _fileMultiAnalysisRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysesQueryByHashHandler"/> class.
    /// </summary>
    /// <param name="fileMultiAnalysisRepository">Repository of file analyses.</param>
    public FileMultiAnalysesQueryByHashHandler(
        IRepository<FileMultiAnalysis, GlobalId> fileMultiAnalysisRepository)
    {
        _fileMultiAnalysisRepository = fileMultiAnalysisRepository;
    }

    /// <summary>
    /// Handles a <see cref="FileMultiAnalysesQueryByHash"/>.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="IEnumerable{FileMultiAnalysis}"/>.</returns>
    public async Task<IReadOnlyList<FileMultiAnalysis>> Handle(FileMultiAnalysesQueryByHash query, CancellationToken cancellationToken)
    {
        Func<IQueryable<FileMultiAnalysis>, IOrderedQueryable<FileMultiAnalysis>>? orderBy = null;
        string dateOrder = query.StartedDateOrder.Trim();
        if (dateOrder.Equals("asc", StringComparison.InvariantCultureIgnoreCase))
        {
            orderBy = q => q.OrderBy(f => f.StartedDate);
        }
        else if (dateOrder.Equals("dsc", StringComparison.InvariantCultureIgnoreCase))
        {
            orderBy = q => q.OrderByDescending(f => f.StartedDate);
        }

        IReadOnlyList<FileMultiAnalysis> fileAnalyses = await _fileMultiAnalysisRepository.GetManyAsync(
            query.FileAnalysesAmount,
            f => f.DataHashSet.Sha256 == query.Hash
                || f.DataHashSet.Md5 == query.Hash
                || f.DataHashSet.Sha1 == query.Hash
                || f.DataHashSet.Sha512 == query.Hash,
            orderBy,
            cancellationToken);

        return fileAnalyses;
    }
}