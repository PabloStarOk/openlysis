using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Infrastructure.Configuration;

namespace Openlysis.Infrastructure.Services.Finders;

/// <summary>
/// Finds the most recent <typeparamref name="TMultiAnalysis"/> for a user, based on analysis type and hash values.
/// </summary>
/// <typeparam name="TAnalysis">Type of the individual analysis, must inherit from <see cref="Analysis"/>.</typeparam>
/// <typeparam name="TMultiAnalysis">Type of the multi-analysis, must inherit from <see cref="MultiAnalysis{TAnalysis}"/>.</typeparam>
internal sealed class RecentMultiAnalysisFinder<TAnalysis, TMultiAnalysis> : IRecentAnalysisFinder<TMultiAnalysis>
    where TAnalysis : Analysis
    where TMultiAnalysis : MultiAnalysis<TAnalysis>
{
    private readonly IOptions<AnalysisReuseOptions> _options;
    private readonly IRepository<TMultiAnalysis> _repository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecentMultiAnalysisFinder{TAnalysis, TMultiAnalysis}"/> class.
    /// </summary>
    /// <param name="options">The analysis reuse options.</param>
    /// <param name="repository">The repository for multi-analysis entities.</param>
    /// <param name="timeProvider">The time provider for retrieving the current time.</param>
    public RecentMultiAnalysisFinder(
        IOptions<AnalysisReuseOptions> options,
        IRepository<TMultiAnalysis> repository,
        TimeProvider timeProvider)
    {
        _options = options;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<ReusableAnalysis<TMultiAnalysis>> FindMostRecentAsync(
        GlobalId userId,
        HashValues hashValues,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset limitDate = now.Subtract(TimeSpan.FromHours(_options.Value.MaxAgeHours));
        TMultiAnalysis? analysis = await _repository.FindAsync(
            filter: m => m.DataHashValues == hashValues,
            orderBy: q => q.OrderByDescending(m => m.StartedDate),
            cancellationToken);

        return new ReusableAnalysis<TMultiAnalysis>(
            IsReusable: analysis is not null
            && analysis.StartedDate > limitDate
            && (!analysis.IsPrivate || analysis.UserId == userId),
            Analysis: analysis);
    }
}