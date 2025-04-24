using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Application.Messages.Contracts.Abstractions;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.EmailAddresses;
using Openlysis.Domain.Files;
using Openlysis.Domain.Messages;
using Openlysis.Domain.Messages.Entities;
using Openlysis.Domain.Phones;
using Openlysis.Domain.URLs;

namespace Openlysis.Infrastructure.Services.Messages;

/// <summary>
/// Updates verdict and status of <see cref="MessageAnalysis"/> objects according to
/// <see cref="FileMultiAnalysis"/>, <see cref="UrlMultiAnalysis"/>, <see cref="EmailAddressMultiReputation"/>
/// and <see cref="PhoneMultiReputation"/> objects.
/// </summary>
internal sealed class MessageAnalysisUpdater : IMessageAnalysisUpdater
{
    private readonly ConcurrentDictionary<GlobalId, UpdatableEntry> _messageAnalysisEntries = [];
    private readonly IServiceScopeFactory _serviceFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAnalysisUpdater"/> class.
    /// </summary>
    /// <param name="serviceFactory">
    /// An <see cref="IServiceScopeFactory"/> used to create service scopes for dependency injection.
    /// </param>
    public MessageAnalysisUpdater(IServiceScopeFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    /// <inheritdoc/>
    public async Task AddPendingAsync(
        MessageAnalysis messageAnalysis,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageAnalysis);

        var entry = UpdatableEntry.Create(messageAnalysis);
        RegisterUpdatableEntry(entry);
        await UpdateWithMultiReputationsAsync(messageAnalysis, cancellationToken);
        CompleteIfNotUpdatable(entry);
        await StoreInRepositoryAsync(messageAnalysis, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task NotifyChildAnalysisStateAsync(
        GlobalId childAnalysisId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(childAnalysisId);

        IEnumerable<UpdatableEntry> updatableEntries =
            FindUpdatableEntries(childAnalysisId);

        foreach (UpdatableEntry entry in updatableEntries)
        {
            await UpdateWithMultiAnalysesAsync(entry, cancellationToken);
            await UpdateInRepositoryAsync(entry.MessageAnalysis, cancellationToken);
            RemoveIfTerminal(entry);
        }
    }

    /// <summary>
    /// Represents an entry that can be updated, containing a <see cref="MessageAnalysis"/> object
    /// and its associated child analysis IDs that are in a terminal status (Completed, Failed or Timed out).
    /// </summary>
    private sealed class UpdatableEntry
    {
        /// <summary>
        /// Gets the <see cref="MessageAnalysis"/> object associated with this instance.
        /// </summary>
        public MessageAnalysis MessageAnalysis { get; }

        /// <summary>
        /// Gets a value indicating whether the associated <see cref="MessageAnalysis"/> object
        /// can be updated based on the presence of updatable child analyses.
        /// </summary>
        public bool IsUpdatable { get; }

        /// <summary>
        /// Gets a read-only list of cached analysis states, representing the terminal states
        /// of child analyses associated with this entry.
        /// </summary>
        public IReadOnlyList<AnalysisState> CachedTerminalAnalysisStates => _cachedTerminalAnalysisStates;

        private readonly List<GlobalId> _cachedTerminalAnalysisIds;
        private readonly List<AnalysisState> _cachedTerminalAnalysisStates;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatableEntry"/> class.
        /// </summary>
        /// <param name="messageAnalysis">The <see cref="MessageAnalysis"/> object associated with this instance.</param>
        /// <param name="updatableAnalysesCount">
        /// The initial capacity for the collection of terminal child analyses,
        /// representing the maximum number of multi analyses that can be updated.
        /// </param>
        private UpdatableEntry(
            MessageAnalysis messageAnalysis,
            int updatableAnalysesCount)
        {
            MessageAnalysis = messageAnalysis;
            _cachedTerminalAnalysisIds = new List<GlobalId>(updatableAnalysesCount);
            _cachedTerminalAnalysisStates = new List<AnalysisState>(updatableAnalysesCount);
            IsUpdatable = HasUpdatableChildAnalyses();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UpdatableEntry"/> class based on the provided <see cref="MessageAnalysis"/> object.
        /// </summary>
        /// <param name="messageAnalysis">The <see cref="MessageAnalysis"/> object to create the entry for.</param>
        /// <returns>A new <see cref="UpdatableEntry"/> instance initialized with the given <see cref="MessageAnalysis"/>.</returns>
        public static UpdatableEntry Create(MessageAnalysis messageAnalysis)
        {
            int updatableAnalysesCount = messageAnalysis.AttachedFilesResults.Count
                + messageAnalysis.DetectedUrlsResults.Count;
            return new UpdatableEntry(
                messageAnalysis,
                updatableAnalysesCount);
        }

        /// <summary>
        /// Finds the IDs of child analyses that are pending (not yet in a terminal state).
        /// </summary>
        /// <returns>An enumerable collection of IDs for pending child analyses.</returns>
        public GlobalId[] GetPendingChildAnalyses<TData>(
            IEnumerable<DataAssessmentResult<TData>> results)
            where TData : notnull
        {
            return results
                .Where(a => !_cachedTerminalAnalysisIds.Contains(a.ResultId))
                .Select(a => a.ResultId)
                .ToArray();
        }

        /// <summary>
        /// Registers terminal child analyses from the provided collection of multi-analyses, but only the ones
        /// that are in terminal state.
        /// </summary>
        /// <typeparam name="TServiceAnalysis">The type of the service analysis contained in the multi-analyses.</typeparam>
        /// <param name="multiAnalyses">A collection of multi-analyses to process and register terminal child analyses from.</param>
        public void CacheTerminalChildAnalyses<TServiceAnalysis>(
            IEnumerable<MultiAnalysis<TServiceAnalysis>> multiAnalyses)
            where TServiceAnalysis : ServiceAnalysis
        {
            ArgumentNullException.ThrowIfNull(multiAnalyses);

            var filteredMultiAnalyses = multiAnalyses
                .Where(f => f.Status is not AnalysisStatus.Queued and not AnalysisStatus.InProgress);

            foreach (var multiAnalysis in filteredMultiAnalyses)
            {
                if (_cachedTerminalAnalysisIds.Contains(multiAnalysis.Id))
                {
                    continue;
                }

                var analysisState = AnalysisState
                    .Initial()
                    .WithVerdict(multiAnalysis.FinalVerdict)
                    .WithStatus(multiAnalysis.Status);

                _cachedTerminalAnalysisIds.Add(multiAnalysis.Id);
                _cachedTerminalAnalysisStates.Add(analysisState);
            }
        }

        /// <summary>
        /// Determines whether the associated <see cref="MessageAnalysis"/> object has any child analyses
        /// that can be updated, based on the presence of attached file results or detected URL results.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there are updatable child analyses; otherwise, <c>false</c>.
        /// </returns>
        private bool HasUpdatableChildAnalyses()
        {
            return MessageAnalysis is
            {
                AttachedFilesResults.Count: > 0,
                DetectedUrlsResults.Count: > 0
            };
        }
    }

    /// <summary>
    /// Completes the associated <see cref="MessageAnalysis"/> if it is not updatable.
    /// Updates the status to <see cref="AnalysisStatus.Completed"/> while retaining the current verdict.
    /// </summary>
    /// <param name="entry">The <see cref="UpdatableEntry"/> to evaluate and potentially complete.</param>
    private static void CompleteIfNotUpdatable(UpdatableEntry entry)
    {
        if (entry.IsUpdatable)
        {
            return;
        }

        Verdict currentVerdict = entry.MessageAnalysis.State.Verdict;
        entry.MessageAnalysis.Update(
            currentVerdict,
            AnalysisStatus.Completed);
    }

    /// <summary>
    /// Calculates the overall verdict by selecting the maximum value from the given collection of verdicts.
    /// </summary>
    /// <param name="verdicts">A collection of verdicts to evaluate.</param>
    /// <returns>The highest verdict from the collection.</returns>
    private static Verdict CalculateVerdict(IEnumerable<Verdict> verdicts)
    {
        return verdicts.Max(v => v);
    }

    /// <summary>
    /// Determines the overall analysis status based on an array of individual statuses.
    /// </summary>
    /// <param name="statuses">An array of analysis statuses to evaluate.</param>
    /// <returns>The calculated overall analysis status.</returns>
    private static AnalysisStatus CalculateStatus(AnalysisStatus[] statuses)
    {
        // If all timeout, set as timeout
        if (statuses.All(s => s is AnalysisStatus.Timeout))
        {
            return AnalysisStatus.Timeout;
        }

        // If all failed, set as failed
        if (statuses.All(s => s is AnalysisStatus.Failed))
        {
            return AnalysisStatus.Failed;
        }

        if (statuses.All(s => s is not AnalysisStatus.Queued and not AnalysisStatus.InProgress)
            && statuses.Any(s => s is AnalysisStatus.Completed))
        {
            return AnalysisStatus.Completed;
        }

        // Queued or in progress according to most frequent or lower status.
        var statusCount = statuses.GroupBy(s => s)
            .ToDictionary(g => g.Key, g => g.Count())
            .Where(g => g.Key is not AnalysisStatus.Completed);

        return statusCount.OrderByDescending(s => s.Value)
            .ThenBy(s => s.Key)
            .First().Key;
    }

    /// <summary>
    /// Updates the given <see cref="MessageAnalysis"/> object with a calculated verdict and status.
    /// </summary>
    /// <param name="analysis">The <see cref="MessageAnalysis"/> object to update.</param>
    /// <param name="verdicts">A collection of verdicts to evaluate for the update.</param>
    /// <param name="statuses">A collection of analysis statuses to evaluate for the update.</param>
    private static void UpdateMessageAnalysis(
        MessageAnalysis analysis,
        IEnumerable<Verdict> verdicts,
        IEnumerable<AnalysisStatus> statuses)
    {
        Verdict verdict = CalculateVerdict(verdicts);
        AnalysisStatus status = CalculateStatus(statuses.ToArray());

        analysis.Update(verdict, status);
    }

    /// <summary>
    /// Adds an updatable entry to the collection of message analysis entries.
    /// </summary>
    /// <param name="entry">The <see cref="UpdatableEntry"/> to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the given <see cref="MessageAnalysis"/> already exists in the collection.
    /// </exception>
    private void RegisterUpdatableEntry(UpdatableEntry entry)
    {
        if (!entry.IsUpdatable)
        {
            return;
        }

        bool wasAdded = _messageAnalysisEntries.TryAdd(
            entry.MessageAnalysis.Id,
            entry);

        if (!wasAdded)
        {
            throw new InvalidOperationException($"Given {typeof(MessageAnalysis)} already exists in the collection.");
        }
    }

    /// <summary>
    /// Stores the given <see cref="MessageAnalysis"/> entity in the repository.
    /// </summary>
    /// <param name="analysis">The <see cref="MessageAnalysis"/> entity to store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task StoreInRepositoryAsync(
        MessageAnalysis analysis,
        CancellationToken cancellationToken)
    {
        await using var scope = _serviceFactory.CreateAsyncScope();
        IRepository<MessageAnalysis, GlobalId> repository = scope.ServiceProvider
            .GetRequiredService<IRepository<MessageAnalysis, GlobalId>>();
        await repository.AddAsync(analysis, cancellationToken);
    }

    /// <summary>
    /// Updates an existing <see cref="MessageAnalysis"/> entity in the repository.
    /// </summary>
    /// <param name="analysis">The <see cref="MessageAnalysis"/> entity to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task UpdateInRepositoryAsync(
        MessageAnalysis analysis,
        CancellationToken cancellationToken)
    {
        await using var scope = _serviceFactory.CreateAsyncScope();
        IRepository<MessageAnalysis, GlobalId> repository = scope.ServiceProvider
            .GetRequiredService<IRepository<MessageAnalysis, GlobalId>>();
        await repository.UpdateAsync(analysis, cancellationToken);
    }

    /// <summary>
    /// Retrieves multiple analysis or reputation results from the database based on the provided IDs.
    /// </summary>
    /// <typeparam name="TModel">The type of the result model to retrieve.</typeparam>
    /// <param name="ids">A collection of IDs to retrieve the results for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An array of retrieved result models.</returns>
    private async Task<TModel[]> RetrieveMultiResultsAsync<TModel>(
        GlobalId[] ids,
        CancellationToken cancellationToken = default)
        where TModel : notnull
    {
        if (ids.Length is 0)
        {
            return [];
        }

        await using var scope = _serviceFactory.CreateAsyncScope();
        IRepository<TModel, GlobalId> repository = scope.ServiceProvider
            .GetRequiredService<IRepository<TModel, GlobalId>>();

        List<TModel> multiAnalyses = [];
        foreach (var id in ids)
        {
            TModel? multiAnalysis =
                await repository.GetAsync(id, cancellationToken);

            if (multiAnalysis is not null)
            {
                multiAnalyses.Add(multiAnalysis);
            }
        }

        return multiAnalyses.ToArray();
    }

    /// <summary>
    /// Updates the given <see cref="MessageAnalysis"/> object with reputations from email addresses
    /// and phone numbers, if applicable.
    /// </summary>
    /// <param name="analysis">The <see cref="MessageAnalysis"/> object to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task UpdateWithMultiReputationsAsync(
        MessageAnalysis analysis,
        CancellationToken cancellationToken = default)
    {
        List<Verdict> verdicts = [];

        if (analysis is
            {
                DetectedEmailAddressesResults.Count: 0,
                DetectedPhoneNumbersResults.Count: 0,
            })
        {
            verdicts.Add(Verdict.Undetected);
        }

        if (analysis.DetectedEmailAddressesResults.Count > 0)
        {
            GlobalId[] resultIds = analysis.DetectedEmailAddressesResults
                .Select(e => e.ResultId)
                .ToArray();

            IEnumerable<EmailAddressMultiReputation> emailMultiReputations =
                await RetrieveMultiResultsAsync<EmailAddressMultiReputation>(
                    resultIds,
                    cancellationToken);

            verdicts.AddRange(emailMultiReputations.Select(e => e.FinalVerdict));
        }

        if (analysis.DetectedPhoneNumbersResults.Count > 0)
        {
            GlobalId[] resultIds = analysis.DetectedPhoneNumbersResults
                .Select(e => e.ResultId)
                .ToArray();

            IEnumerable<PhoneMultiReputation> phoneMultiReputations =
                await RetrieveMultiResultsAsync<PhoneMultiReputation>(
                    resultIds,
                    cancellationToken);

            verdicts.AddRange(phoneMultiReputations.Select(e => e.FinalVerdict));
        }

        UpdateMessageAnalysis(analysis, verdicts, [AnalysisStatus.Queued]);
    }

    /// <summary>
    /// Updates the specified <see cref="UpdatableEntry"/> with the latest multi-analysis results,
    /// including verdicts and statuses, and updates the associated <see cref="MessageAnalysis"/> object.
    /// </summary>
    /// <param name="entry">The <see cref="UpdatableEntry"/> to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task UpdateWithMultiAnalysesAsync(
        UpdatableEntry entry,
        CancellationToken cancellationToken = default)
    {
        List<Verdict> verdicts = [];
        List<AnalysisStatus> statuses = [];
        MessageAnalysis analysis = entry.MessageAnalysis;

        verdicts.AddRange(entry.CachedTerminalAnalysisStates.Select(a => a.Verdict));
        statuses.AddRange(entry.CachedTerminalAnalysisStates.Select(a => a.Status));

        if (analysis.AttachedFilesResults.Count > 0)
        {
            GlobalId[] pendingAnalysisIds
                = entry.GetPendingChildAnalyses(analysis.AttachedFilesResults);

            FileMultiAnalysis[] fileMultiAnalyses =
                await RetrieveMultiResultsAsync<FileMultiAnalysis>(
                pendingAnalysisIds,
                cancellationToken);

            entry.CacheTerminalChildAnalyses(fileMultiAnalyses);

            verdicts.AddRange(fileMultiAnalyses.Select(f => f.FinalVerdict));
            statuses.AddRange(fileMultiAnalyses.Select(f => f.Status));
        }

        if (analysis.DetectedUrlsResults.Count > 0)
        {
            GlobalId[] pendingAnalysisIds
                = entry.GetPendingChildAnalyses(analysis.DetectedUrlsResults);

            UrlMultiAnalysis[] urlMultiAnalyses =
                await RetrieveMultiResultsAsync<UrlMultiAnalysis>(
                pendingAnalysisIds,
                cancellationToken);

            entry.CacheTerminalChildAnalyses(urlMultiAnalyses);

            verdicts.AddRange(urlMultiAnalyses.Select(f => f.FinalVerdict));
            statuses.AddRange(urlMultiAnalyses.Select(f => f.Status));
        }

        UpdateMessageAnalysis(analysis, verdicts, statuses);
    }

    /// <summary>
    /// Finds all message analyses that can be updated based on the given child analysis ID.
    /// </summary>
    /// <param name="childAnalysisId">The ID of the child analysis to search for.</param>
    /// <returns>An enumerable collection of message analyses that are updatable.</returns>
    private IEnumerable<UpdatableEntry> FindUpdatableEntries(
        GlobalId childAnalysisId)
    {
        return _messageAnalysisEntries.Values
            .Where(
                m =>
                {
                    return m.MessageAnalysis.AttachedFilesResults
                            .Any(a => a.ResultId == childAnalysisId)
                        || m.MessageAnalysis.DetectedUrlsResults
                            .Any(a => a.ResultId == childAnalysisId);
                });
    }

    /// <summary>
    /// Removes the specified <see cref="UpdatableEntry"/> from the collection if its associated
    /// <see cref="MessageAnalysis"/> has reached a terminal status (Completed, Failed, or Timed out).
    /// </summary>
    /// <param name="entry">The <see cref="UpdatableEntry"/> to evaluate and potentially remove.</param>
    private void RemoveIfTerminal(UpdatableEntry entry)
    {
        if (entry.MessageAnalysis.State.Status
            is AnalysisStatus.Completed
            or AnalysisStatus.Failed
            or AnalysisStatus.Timeout)
        {
            _messageAnalysisEntries.TryRemove(entry.MessageAnalysis.Id, out _);
        }
    }
}