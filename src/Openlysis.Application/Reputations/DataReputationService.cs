using System.Collections.Concurrent;

using ErrorOr;

using Microsoft.Extensions.Logging;

using Openlysis.Application.Common.Abstractions.Contracts;
using Openlysis.Application.Common.Abstractions.Persistence;
using Openlysis.Domain.Common.Aggregates;
using Openlysis.Domain.Common.Entities;

namespace Openlysis.Application.Reputations;

/// <summary>
/// Represents an abstract base class for a data reputation service.
/// </summary>
/// <typeparam name="TData">The type of the input data used for reputation evaluation.</typeparam>
/// <typeparam name="TServiceReputation">The type of the service reputation objects.</typeparam>
/// <typeparam name="TMultiReputation">The type of the multi-reputation object that aggregates service reputations.</typeparam>
internal abstract class DataReputationService<
    TData,
    TServiceReputation,
    TMultiReputation>
    where TData : notnull
    where TServiceReputation : Reputation
    where TMultiReputation : MultiReputation<TServiceReputation>
{
    /// <summary>
    /// Gets a value indicating whether any of the reputation evaluators are available.
    /// </summary>
    public bool IsAvailable => _reputationEvaluators.Any(r => r.IsAvailable);

    /// <summary>
    /// Gets the time provider used for accessing time-related functionality.
    /// </summary>
    protected TimeProvider TimeProvider { get; }

    private readonly ILogger<DataReputationService<TData, TServiceReputation, TMultiReputation>> _logger;
    private readonly IRepository<TMultiReputation> _repository;
    private readonly IEnumerable<IReputationEvaluator<TData, TServiceReputation>> _reputationEvaluators;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataReputationService{TData, TServiceReputation, TMultiReputation}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging information and errors.</param>
    /// <param name="repository">The repository for storing and retrieving multi-reputation objects.</param>
    /// <param name="timeProvider">Provides access to time-related functionality.</param>
    /// <param name="reputationEvaluators">A collection of reputation evaluators used for evaluating data.</param>
    protected DataReputationService(
        ILogger<DataReputationService<TData, TServiceReputation, TMultiReputation>> logger,
        IRepository<TMultiReputation> repository,
        TimeProvider timeProvider,
        IEnumerable<IReputationEvaluator<TData, TServiceReputation>> reputationEvaluators)
    {
        _logger = logger;
        _repository = repository;
        TimeProvider = timeProvider;
        _reputationEvaluators = reputationEvaluators;
    }

    /// <summary>
    /// Retrieves a multi-reputation object based on the provided data.
    /// </summary>
    /// <param name="data">The input data used for reputation evaluation.</param>
    /// <param name="storeInDatabase">Indicates whether the result should be stored in the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing the multi-reputation object if successful,
    /// or a collection of errors if the operation fails.
    /// </returns>
    public async Task<ErrorOr<TMultiReputation>> GetAsync(
        TData data,
        bool storeInDatabase,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            Error.Failure("Service is not available.");
        }

        ErrorOr<TMultiReputation> evaluateResult = await EvaluateAsync(
            data,
            cancellationToken);

        if (evaluateResult.IsError)
        {
            return evaluateResult;
        }

        TMultiReputation multiReputation = evaluateResult.Value;

        if (multiReputation.Reputations.Count is 0)
        {
            string errorMessage = $"{typeof(TMultiReputation)} object doesn't contain {typeof(TServiceReputation)} objects and none error was returned by evaluator services.";
            _logger.LogError(errorMessage);
            return Error.Unexpected(description: errorMessage);
        }

        if (storeInDatabase)
        {
            await _repository.AddAsync(multiReputation, cancellationToken);
            await _repository.SaveChangeAsync(cancellationToken);
        }

        return multiReputation;
    }

    /// <summary>
    /// Creates a new instance of the <typeparamref name="TMultiReputation"/> object
    /// based on the provided data.
    /// </summary>
    /// <param name="data">The input data used to create the multi-reputation object.</param>
    /// <returns>A new instance of <typeparamref name="TMultiReputation"/>.</returns>
    protected abstract TMultiReputation CreateMultiReputation(TData data);

    /// <summary>
    /// Evaluates the provided data using all available reputation evaluators.
    /// </summary>
    /// <param name="data">The input data to be evaluated.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing the multi-reputation object if successful,
    /// or a collection of errors if the evaluation fails.
    /// </returns>
    private async Task<ErrorOr<TMultiReputation>> EvaluateAsync(
        TData data,
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];
        TMultiReputation multiReputation = CreateMultiReputation(data);

        // TODO: When service is unavailable, use a timeout to listen for an event from the service to know when it is available again.
        ConcurrentBag<TServiceReputation> serviceReputations = [];
        await Parallel.ForEachAsync(_reputationEvaluators, cancellationToken, async (evaluator, ct) =>
        {
            if (!evaluator.IsAvailable)
            {
                return;
            }

            ErrorOr<TServiceReputation> result = await evaluator
                .EvaluateAsync(data, ct);

            if (result.IsError)
            {
                errors.AddRange(result.Errors);
                _logger.LogError(
                    "Error received from {EvaluatorName}"
                    + "\n\tIs Service Available: {IsAvailable}"
                    + "\n\tErrors: {Errors}",
                    evaluator.ServiceName,
                    evaluator.IsAvailable,
                    errors);
                return;
            }

            serviceReputations.Add(result.Value);
        });

        foreach (var reputation in serviceReputations)
        {
            multiReputation.AddReputation(reputation);
        }

        return errors.Count > 0 ? errors : multiReputation;
    }
}