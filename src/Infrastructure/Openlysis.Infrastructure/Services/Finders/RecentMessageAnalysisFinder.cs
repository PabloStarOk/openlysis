using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Application.Common.Models;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Messages;
using Openlysis.Infrastructure.Configuration;

namespace Openlysis.Infrastructure.Services.Finders;

/// <summary>
/// Provides functionality to find the most recent <see cref="MessageAnalysis"/> for a user,
/// based on message hash values and analysis reuse options.
/// </summary>
internal sealed class RecentMessageAnalysisFinder : IRecentAnalysisFinder<MessageAnalysis>
{
    private readonly IOptions<AnalysisReuseOptions> _options;
    private readonly IRepository<MessageAnalysis> _repository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecentMessageAnalysisFinder"/> class.
    /// </summary>
    /// <param name="options">The analysis reuse options.</param>
    /// <param name="repository">The repository for <see cref="MessageAnalysis"/> entities.</param>
    /// <param name="timeProvider">The time provider for retrieving the current UTC time.</param>
    public RecentMessageAnalysisFinder(
        IOptions<AnalysisReuseOptions> options,
        IRepository<MessageAnalysis> repository,
        TimeProvider timeProvider)
    {
        _options = options;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<ReusableAnalysis<MessageAnalysis>> FindMostRecentAsync(
        GlobalId userId,
        HashValues hashValues,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset limitDate = now.Subtract(TimeSpan.FromHours(_options.Value.MaxAgeHours));
        MessageAnalysis? analysis = await _repository.FindAsync(
            filter: m => m.Message.HashValues == hashValues,
            orderBy: q => q.OrderByDescending(m => m.StartedDate),
            cancellationToken);

        return new ReusableAnalysis<MessageAnalysis>(
            IsReusable: analysis is not null
            && analysis.StartedDate > limitDate
            && (!analysis.IsPrivate || analysis.UserId == userId),
            Analysis: analysis);
    }
}