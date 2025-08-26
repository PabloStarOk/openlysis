using ErrorOr;

using Openlysis.Application.Common.Enums;
using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;

namespace Openlysis.Application.Files.Services;

/// <summary>
/// Defines a multi-analysis service for files.
/// </summary>
public interface IFileMultiAnalysisService
{
    /// <summary>
    /// Analyzes a file based on the provided request.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the analysis.</param>
    /// <param name="processedFile">The processed file data to be analyzed.</param>
    /// <param name="filePassword">The password for the file, if required for analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="reanalyze">Specifies whether to reanalyze the file if it has been analyzed before.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The result is an <see cref="ErrorOr{T}"/> containing the analysis request outcome.
    /// </returns>
    public Task<ErrorOr<AnalysisRequestResult<FileMultiAnalysis>>> AnalyzeAsync(
        GlobalId userId,
        ProcessedFile processedFile,
        string filePassword,
        bool isPrivate,
        bool reanalyze,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a file analysis by its unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the analysis.</param>
    /// <param name="id">The unique identifier of the file analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an <see cref="ErrorOr{T}"/> object with the file analysis.
    /// </returns>
    public Task<ErrorOr<FileMultiAnalysis>> GetAnalysisByIdAsync(
        GlobalId userId,
        GlobalId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of file analyses based on the provided hash.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the analyses.</param>
    /// <param name="hash">The hash value used to filter the analyses.</param>
    /// <param name="page">The page number for pagination (starting from 1).</param>
    /// <param name="pageSize">The number of analyses to retrieve per page.</param>
    /// <param name="order">The order in which the analyses should be returned.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an <see cref="ErrorOr{T}"/> object with a read-only list of file analyses.
    /// </returns>
    public Task<IReadOnlyList<FileMultiAnalysis>> GetAnalysesByHashAsync(
        GlobalId userId,
        string hash,
        int page,
        int pageSize,
        OrderType order,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of file multi analyses performed by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose file multi analyses are to be retrieved.</param>
    /// <param name="page">The page number of the results to retrieve.</param>
    /// <param name="pageSize">The number of file multi analyses per page.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of <see cref="FileMultiAnalysis"/> objects for the specified user.</returns>
    public Task<IReadOnlyList<FileMultiAnalysis>> GetAnalysesByUserAsync(
        GlobalId userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}