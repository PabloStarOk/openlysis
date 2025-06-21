using ErrorOr;

using Openlysis.Application.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs;
using Openlysis.Domain.Users.ValueObjects;

namespace Openlysis.Application.URLs.Services;

/// <summary>
/// Defines a multi-analysis service for URLs.
/// </summary>
public interface IUrlMultiAnalysisService
{
    /// <summary>
    /// Analyzes a URL and returns the result of the analysis.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the analysis.</param>
    /// <param name="isPrivate">Indicates whether the analysis is private.</param>
    /// <param name="url">The URL to be analyzed.</param>
    /// <param name="reanalyze">Indicates whether to reanalyze the URL even if it has been analyzed before.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing the analysis result or an error.</returns>
    public Task<ErrorOr<UrlMultiAnalysis>> AnalyzeAsync(
        UserId userId,
        bool isPrivate,
        Uri url,
        bool reanalyze,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a URL analysis by its unique identifier.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the analysis.</param>
    /// <param name="id">The unique identifier of the URL analysis to retrieve.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing the analysis result or an error.</returns>
    public Task<ErrorOr<UrlMultiAnalysis>> GetAnalysisByIdAsync(
        UserId userId,
        GlobalId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of URL analyses by their hash value.
    /// </summary>
    /// <param name="userId">The ID of the user requesting the analyses.</param>
    /// <param name="hash">The hash value of the analyses to retrieve.</param>
    /// <param name="pageSize">The page number for pagination (starting from 1).</param>
    /// <param name="size">The number of analyses to retrieve per page.</param>
    /// <param name="order">The order in which to retrieve the analyses.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of <see cref="UrlMultiAnalysis"/> objects.</returns>
    public Task<IReadOnlyList<UrlMultiAnalysis>> GetAnalysesByHashAsync(
        UserId userId,
        string hash,
        int pageSize,
        int size,
        OrderType order,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of URL analyses performed by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose analyses are to be retrieved.</param>
    /// <param name="page">The page number of the results to retrieve.</param>
    /// <param name="pageSize">The number of analyses per page.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of <see cref="UrlMultiAnalysis"/> objects for the specified user.</returns>
    public Task<IReadOnlyList<UrlMultiAnalysis>> GetAnalysesByUserAsync(
        UserId userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}