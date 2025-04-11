using ErrorOr;

using Openlysis.Application.Common.Enums;
using Openlysis.Application.Files.Contracts.Models;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files;
using Openlysis.Domain.Users.ValueObjects;

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
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="reanalyze">Specifies whether to reanalyze the file if it has been analyzed before.</param>
    /// <param name="fileData">The file data to be analyzed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an <see cref="ErrorOr{T}"/> object with the file analysis result.
    /// </returns>
    public Task<ErrorOr<FileMultiAnalysis>> AnalyzeAsync(
        UserId userId,
        bool isPrivate,
        bool reanalyze,
        FileData fileData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a file analysis by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the file analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an <see cref="ErrorOr{T}"/> object with the file analysis.
    /// </returns>
    public Task<ErrorOr<FileMultiAnalysis>> GetAnalysisByIdAsync(
        GlobalId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of file analyses based on the provided hash.
    /// </summary>
    /// <param name="hash">The hash value used to filter the analyses.</param>
    /// <param name="amount">The maximum number of analyses to retrieve.</param>
    /// <param name="order">The order in which the analyses should be returned.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an <see cref="ErrorOr{T}"/> object with a read-only list of file analyses.
    /// </returns>
    public Task<IReadOnlyList<FileMultiAnalysis>> GetAnalysesByHashAsync(
        string hash,
        int amount,
        OrderType order,
        CancellationToken cancellationToken = default);
}