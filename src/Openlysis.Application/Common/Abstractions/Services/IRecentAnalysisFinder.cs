using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a service for finding the most recent analysis for a user based on hash values.
/// </summary>
/// <typeparam name="TAnalysis">
/// The type of analysis entity, constrained to <see cref="Entity{GlobalId}"/>.
/// </typeparam>
public interface IRecentAnalysisFinder<TAnalysis>
    where TAnalysis : Entity<GlobalId>
{
    /// <summary>
    /// Finds the most recent analysis for the specified user and hash values.
    /// </summary>
    /// <param name="userId">The global ID of the user.</param>
    /// <param name="hashValues">The hash values to filter analyses.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the most recent analysis or <c>null</c> if none found.
    /// </returns>
    public Task<ReusableAnalysis<TAnalysis>> FindMostRecentAsync(
        GlobalId userId,
        HashValues hashValues,
        CancellationToken cancellationToken = default);
}